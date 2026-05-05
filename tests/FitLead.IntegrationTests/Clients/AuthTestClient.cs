using System.Net.Http.Json;
using FitLead.Api.Auth.Contracts;
using FitLead.IntegrationTests.Helpers;

namespace FitLead.IntegrationTests.Clients;

public sealed class AuthTestClient(HttpClient httpClient)
{
    public Task<HttpResponseMessage> RegisterAsync(
        string email,
        string password,
        string fullName,
        string role,
        CancellationToken cancellationToken = default)
    {
        var request = new RegisterRequest(email, password, fullName, role);
        return httpClient.PostAsJsonAsync("/auth/register", request, cancellationToken);
    }

    public Task<HttpResponseMessage> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest(email, password);
        return httpClient.PostAsJsonAsync("/auth/login", request, cancellationToken);
    }

    public Task<HttpResponseMessage> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        return httpClient.PostAsync("/auth/refresh", content: null, cancellationToken);
    }

    public Task<HttpResponseMessage> RefreshWithRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        request.Headers.Add("Cookie", $"{AuthCookieNames.RefreshToken}={refreshToken}");
        return httpClient.SendAsync(request, cancellationToken);
    }

    public Task<HttpResponseMessage> LogoutAsync(
        CancellationToken cancellationToken = default)
    {
        return httpClient.PostAsync("/auth/logout", content: null, cancellationToken);
    }
}
