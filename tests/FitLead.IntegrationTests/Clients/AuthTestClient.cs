using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitLead.Api.Auth.Contracts;

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
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var request = new RefreshRequest(refreshToken);
        return httpClient.PostAsJsonAsync("/auth/refresh", request, cancellationToken);
    }

    public void SetBearerToken(string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public void ClearBearerToken()
    {
        httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
