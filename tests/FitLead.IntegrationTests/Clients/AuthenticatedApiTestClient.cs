using System.Net;
using System.Net.Http.Json;
using FitLead.IntegrationTests.Helpers;

namespace FitLead.IntegrationTests.Clients;

public abstract class AuthenticatedApiTestClient
{
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieJar = new();
    private readonly Uri _baseUri;
    private readonly string _clientName;

    protected AuthenticatedApiTestClient(HttpClient httpClient, string clientName)
    {
        _httpClient = httpClient;
        _baseUri = httpClient.BaseAddress ?? new Uri("http://localhost", UriKind.Absolute);
        _clientName = clientName;
    }

    public async Task CopyAuthStateFromAsync(
        AuthTestClient authClient,
        CancellationToken cancellationToken = default)
    {
        var authStateResponse = await authClient.GetAsync("/auth/current-user", cancellationToken);
        authStateResponse.EnsureSuccessStatusCode();

        var requestMessage = authStateResponse.RequestMessage
            ?? throw new InvalidOperationException("Auth current-user response did not include a request message.");

        if (!requestMessage.Headers.TryGetValues("Cookie", out var cookieValues))
        {
            throw new InvalidOperationException("Auth test client did not send cookies on authenticated request.");
        }

        foreach (var cookieHeader in cookieValues)
        {
            if (string.IsNullOrWhiteSpace(cookieHeader))
                continue;

            ApplyCookieHeader(cookieHeader);
        }

        await RefreshCsrfStateAsync(cancellationToken);
    }

    protected Task<HttpResponseMessage> SendGetAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, path);
        return SendAsync(request, includeCsrfHeader: false, cancellationToken);
    }

    protected Task<HttpResponseMessage> SendUnsafeAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        var request = CreateRequest(method, path);
        return SendAsync(request, includeCsrfHeader, cancellationToken);
    }

    protected Task<HttpResponseMessage> SendUnsafeJsonAsync<TPayload>(
        HttpMethod method,
        string path,
        TPayload payload,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        var request = CreateRequest(method, path);
        request.Content = JsonContent.Create(payload);
        return SendAsync(request, includeCsrfHeader, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        bool includeCsrfHeader,
        CancellationToken cancellationToken)
    {
        if (includeCsrfHeader)
        {
            await EnsureCsrfTokenAsync(cancellationToken);
            request.Headers.Add(ApiCsrfTokenNames.RequestHeader, GetRequiredCookieValue(ApiCsrfTokenNames.RequestTokenCookie));
        }

        AddCookieHeader(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        CaptureCookies(response);
        return response;
    }

    private async Task EnsureCsrfTokenAsync(CancellationToken cancellationToken)
    {
        if (HasRequiredCsrfCookies())
            return;

        await RefreshCsrfStateAsync(cancellationToken);
    }

    private async Task RefreshCsrfStateAsync(CancellationToken cancellationToken)
    {
        var request = CreateRequest(HttpMethod.Get, "/auth/csrf-token");
        AddCookieHeader(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        CaptureCookies(response);
    }

    private void ApplyCookieHeader(string cookieHeader)
    {
        var parts = cookieHeader.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var nameValue = part.Split('=', 2);
            if (nameValue.Length == 2)
            {
                _cookieJar.Add(_baseUri, new Cookie(nameValue[0], nameValue[1], "/", _baseUri.Host));
            }
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
        => new(method, new Uri(_baseUri, path));

    private void AddCookieHeader(HttpRequestMessage request)
    {
        var cookies = _cookieJar.GetCookies(request.RequestUri!);
        if (cookies.Count == 0)
            return;

        var header = string.Join("; ",
            cookies.Cast<Cookie>()
                .Where(cookie => !string.IsNullOrWhiteSpace(cookie.Value))
                .Select(cookie => $"{cookie.Name}={cookie.Value}"));

        if (!string.IsNullOrWhiteSpace(header))
        {
            request.Headers.Add("Cookie", header);
        }
    }

    private void CaptureCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            return;

        foreach (var value in values)
        {
            _cookieJar.SetCookies(_baseUri, value);
        }
    }

    private string GetRequiredCookieValue(string cookieName)
    {
        var cookie = _cookieJar.GetCookies(_baseUri)[cookieName];
        if (cookie is null || string.IsNullOrWhiteSpace(cookie.Value))
        {
            throw new InvalidOperationException($"Cookie '{cookieName}' is missing from the {_clientName} test cookie jar.");
        }

        return cookie.Value;
    }

    private bool HasCookie(string cookieName)
    {
        var cookie = _cookieJar.GetCookies(_baseUri)[cookieName];
        return cookie is not null && !string.IsNullOrWhiteSpace(cookie.Value);
    }

    private bool HasRequiredCsrfCookies()
        => HasCookie(ApiCsrfTokenNames.RequestTokenCookie)
            && HasCookie(ApiCsrfTokenNames.AntiforgeryCookie);
}
