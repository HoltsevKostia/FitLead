using System.Net;
using System.Net.Http.Json;
using FitLead.Api.Contracts.Auth;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class AuthSmokeTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Register_ShouldReturnTokens_AndAccessProtectedCurrentUser()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = $"trainer-{Guid.NewGuid():N}@test.local";

        var registerResponse = await authClient.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Test Trainer",
            "Trainer");

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();

        authClient.SetBearerToken(payload.AccessToken);
        var currentUserResponse = await HttpClient.GetAsync("/auth/current-user");

        currentUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = $"client-{Guid.NewGuid():N}@test.local";

        await authClient.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Test Client",
            "Client");

        var loginResponse = await authClient.LoginAsync(email, "wrong-password");

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
