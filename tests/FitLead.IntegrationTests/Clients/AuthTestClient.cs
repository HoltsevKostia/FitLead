using System.Net.Http.Json;
using System.Net;
using FitLead.Api.Auth.Contracts;
using FitLead.IntegrationTests.Helpers;

namespace FitLead.IntegrationTests.Clients;

public sealed class AuthTestClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly CookieContainer _cookieJar = new();
    private readonly Uri _baseUri = httpClient.BaseAddress ?? new Uri("http://localhost", UriKind.Absolute);

    public async Task<string> EnsureCsrfTokenAsync(
        CancellationToken cancellationToken = default)
    {
        if (HasCookie(ApiCsrfTokenNames.RequestTokenCookie))
        {
            return GetRequiredCookieValue(ApiCsrfTokenNames.RequestTokenCookie);
        }

        return await RefreshCsrfTokenAsync(cancellationToken);
    }

    public async Task<string> RefreshCsrfTokenAsync(
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, "/auth/csrf-token");
        var response = await SendAsync(request, includeCsrfHeader: false, cancellationToken);
        response.EnsureSuccessStatusCode();

        return GetRequiredCookieValue(ApiCsrfTokenNames.RequestTokenCookie);
    }

    public Task<HttpResponseMessage> RegisterAsync(
        string email,
        string password,
        string fullName,
        string role,
        CancellationToken cancellationToken = default)
    {
        var request = new RegisterRequest(email, password, fullName, role);
        return SendUnsafeJsonAsync("/auth/register", request, cancellationToken, refreshCsrfAfterSuccess: true);
    }

    public Task<HttpResponseMessage> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest(email, password);
        return SendUnsafeJsonAsync("/auth/login", request, cancellationToken, refreshCsrfAfterSuccess: true);
    }

    public Task<HttpResponseMessage> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeRequestAsync(HttpMethod.Post, "/auth/refresh", cancellationToken);
    }

    public Task<HttpResponseMessage> RefreshWithRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeRequestAsync(
            HttpMethod.Post,
            "/auth/refresh",
            cancellationToken,
            new Dictionary<string, string>
            {
                [AuthCookieNames.RefreshToken] = refreshToken
            });
    }

    public Task<HttpResponseMessage> LogoutAsync(
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeRequestAsync(HttpMethod.Post, "/auth/logout", cancellationToken);
    }

    public Task<HttpResponseMessage> GetAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, path);
        return SendAsync(request, includeCsrfHeader: false, cancellationToken);
    }

    public string GetCookieHeader(string path = "/")
    {
        return BuildCookieHeader(new Uri(_baseUri, path));
    }

    private async Task<HttpResponseMessage> SendUnsafeJsonAsync<TPayload>(
        string path,
        TPayload payload,
        CancellationToken cancellationToken,
        bool refreshCsrfAfterSuccess = false)
    {
        await EnsureCsrfTokenAsync(cancellationToken);

        var request = CreateRequest(HttpMethod.Post, path);
        request.Content = JsonContent.Create(payload);

        var response = await SendAsync(request, includeCsrfHeader: true, cancellationToken);

        if (refreshCsrfAfterSuccess && response.IsSuccessStatusCode)
        {
            await RefreshCsrfTokenAsync(cancellationToken);
        }

        return response;
    }

    private async Task<HttpResponseMessage> SendUnsafeRequestAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string>? cookieOverrides = null)
    {
        await EnsureCsrfTokenAsync(cancellationToken);

        var request = CreateRequest(method, path);
        return await SendAsync(request, includeCsrfHeader: true, cancellationToken, cookieOverrides);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        bool includeCsrfHeader,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string>? cookieOverrides = null)
    {
        AddCookieHeader(request, cookieOverrides);

        if (includeCsrfHeader)
        {
            request.Headers.Add(ApiCsrfTokenNames.RequestHeader, GetRequiredCookieValue(ApiCsrfTokenNames.RequestTokenCookie));
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        CaptureCookies(response);
        return response;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
        => new(method, new Uri(_baseUri, path));

    private void AddCookieHeader(
        HttpRequestMessage request,
        IReadOnlyDictionary<string, string>? cookieOverrides = null)
    {
        var cookieHeader = BuildCookieHeader(request.RequestUri!, cookieOverrides);
        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }
    }

    private string BuildCookieHeader(
        Uri requestUri,
        IReadOnlyDictionary<string, string>? cookieOverrides = null)
    {
        var cookies = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (Cookie cookie in _cookieJar.GetCookies(requestUri))
        {
            if (!string.IsNullOrWhiteSpace(cookie.Value))
            {
                cookies[cookie.Name] = cookie.Value;
            }
        }

        if (cookieOverrides is not null)
        {
            foreach (var overrideCookie in cookieOverrides)
            {
                if (string.IsNullOrWhiteSpace(overrideCookie.Value))
                {
                    cookies.Remove(overrideCookie.Key);
                }
                else
                {
                    cookies[overrideCookie.Key] = overrideCookie.Value;
                }
            }
        }

        return string.Join("; ", cookies.Select(x => $"{x.Key}={x.Value}"));
    }

    private void CaptureCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            return;
        }

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
            throw new InvalidOperationException($"Cookie '{cookieName}' is missing from the local test cookie jar.");
        }

        return cookie.Value;
    }

    private bool HasCookie(string cookieName)
    {
        var cookie = _cookieJar.GetCookies(_baseUri)[cookieName];
        return cookie is not null && !string.IsNullOrWhiteSpace(cookie.Value);
    }
}
