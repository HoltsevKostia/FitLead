using System.Net;
using FitLead.Api.Contracts.Auth;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class RefreshTokenTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Refresh_WithValidToken_ShouldRotateAndReturnNewTokens()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("refresh");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Refresh User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerPayload = await register.ReadRequiredJsonAsync<RegisterResponse>();

        var refresh = await authClient.RefreshAsync(registerPayload.RefreshToken);

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await refresh.ReadRequiredJsonAsync<RefreshResponse>();
        payload.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBe(registerPayload.RefreshToken);

        authClient.SetBearerToken(payload.AccessToken);
        var currentUser = await HttpClient.GetAsync("/auth/current-user");
        currentUser.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_WithReusedToken_ShouldRevokeTokenFamily()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("refresh-reuse");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Refresh Reuse User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerPayload = await register.ReadRequiredJsonAsync<RegisterResponse>();

        var firstRefresh = await authClient.RefreshAsync(registerPayload.RefreshToken);
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstRefreshPayload = await firstRefresh.ReadRequiredJsonAsync<RefreshResponse>();

        var secondRefresh = await authClient.RefreshAsync(registerPayload.RefreshToken);
        secondRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var familyTokenAfterReuse = await authClient.RefreshAsync(firstRefreshPayload.RefreshToken);
        familyTokenAfterReuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var authClient = new AuthTestClient(HttpClient);

        var response = await authClient.RefreshAsync("invalid-refresh-token");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
