using System.Net;
using System.Net.Http.Json;
using FitLead.Api.Invitations.Contracts;
using FitLead.IntegrationTests.Helpers;

namespace FitLead.IntegrationTests.Clients;

public sealed class InvitationsTestClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly CookieContainer _cookieJar = new();
    private readonly Uri _baseUri = httpClient.BaseAddress ?? new Uri("http://localhost", UriKind.Absolute);

    public async Task<HttpResponseMessage> CreateAsync(
        int expiresInDays,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateInvitationRequest(expiresInDays);
        return await SendUnsafeJsonAsync("/api/invitations", request, cancellationToken);
    }

    public async Task<HttpResponseMessage> CreateWithoutCsrfAsync(
        int expiresInDays,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateInvitationRequest(expiresInDays);
        return await SendUnsafeJsonAsync("/api/invitations", request, cancellationToken, includeCsrfHeader: false);
    }

    public Task<HttpResponseMessage> GetTrainerInvitationsAsync(
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, "/api/invitations/trainer");
        return SendAsync(request, includeCsrfHeader: false, cancellationToken);
    }

    public Task<HttpResponseMessage> PreviewAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, $"/api/invitations/{Uri.EscapeDataString(token)}/preview");
        return SendAsync(request, includeCsrfHeader: false, cancellationToken);
    }

    public async Task<HttpResponseMessage> AcceptAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await SendUnsafeRequestAsync(
            HttpMethod.Post,
            $"/api/invitations/{Uri.EscapeDataString(token)}/accept",
            cancellationToken);
    }

    public async Task<HttpResponseMessage> AcceptWithoutCsrfAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await SendUnsafeRequestAsync(
            HttpMethod.Post,
            $"/api/invitations/{Uri.EscapeDataString(token)}/accept",
            cancellationToken,
            includeCsrfHeader: false);
    }

    public async Task<HttpResponseMessage> RevokeAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        return await SendUnsafeRequestAsync(
            HttpMethod.Post,
            $"/api/invitations/{invitationId:D}/revoke",
            cancellationToken);
    }

    public async Task<HttpResponseMessage> RevokeWithoutCsrfAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        return await SendUnsafeRequestAsync(
            HttpMethod.Post,
            $"/api/invitations/{invitationId:D}/revoke",
            cancellationToken,
            includeCsrfHeader: false);
    }

    public async Task CopyAuthStateFromAsync(
        AuthTestClient authClient,
        CancellationToken cancellationToken = default)
    {
        await RefreshAuthStateFromCookieJarAsync(authClient, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendUnsafeJsonAsync<TPayload>(
        string path,
        TPayload payload,
        CancellationToken cancellationToken,
        bool includeCsrfHeader = true)
    {
        var request = CreateRequest(HttpMethod.Post, path);
        request.Content = JsonContent.Create(payload);
        return await SendAsync(request, includeCsrfHeader, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendUnsafeRequestAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken,
        bool includeCsrfHeader = true)
    {
        var request = CreateRequest(method, path);
        return await SendAsync(request, includeCsrfHeader, cancellationToken);
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
        {
            return;
        }

        await RefreshCsrfStateAsync(cancellationToken);
    }

    private async Task RefreshAuthStateFromCookieJarAsync(
        AuthTestClient authClient,
        CancellationToken cancellationToken)
    {
        var authStateResponse = await authClient.GetAsync("/auth/current-user", cancellationToken);
        authStateResponse.EnsureSuccessStatusCode();

        if (authStateResponse.RequestMessage?.Headers.TryGetValues("Cookie", out var cookieValues) != true)
        {
            throw new InvalidOperationException("Auth test client did not send cookies on authenticated request.");
        }

        foreach (var cookieHeader in cookieValues)
        {
            ApplyCookieHeader(cookieHeader);
        }

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
            if (nameValue.Length != 2)
            {
                continue;
            }

            _cookieJar.Add(_baseUri, new Cookie(nameValue[0], nameValue[1], "/", _baseUri.Host));
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
        => new(method, new Uri(_baseUri, path));

    private void AddCookieHeader(HttpRequestMessage request)
    {
        var cookies = _cookieJar.GetCookies(request.RequestUri!);
        if (cookies.Count == 0)
        {
            return;
        }

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
            throw new InvalidOperationException($"Cookie '{cookieName}' is missing from the invitation test cookie jar.");
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
