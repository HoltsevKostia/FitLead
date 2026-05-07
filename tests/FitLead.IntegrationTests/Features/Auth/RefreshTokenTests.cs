using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class RefreshTokenTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Refresh_WithValidCookie_ShouldRotateAndIssueNewAuthCookies()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("refresh");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Refresh User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerRefreshCookie = register.GetRequiredCookie(AuthCookieNames.RefreshToken);

        var refresh = await authClient.RefreshAsync();

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await refresh.ReadRequiredJsonAsync<AuthSessionResponse>();
        payload.ExpiresIn.Should().BePositive();

        var accessCookie = refresh.GetRequiredCookie(AuthCookieNames.AccessToken);
        accessCookie.Value.Should().NotBeNullOrWhiteSpace();
        accessCookie.HttpOnly.Should().BeTrue();
        accessCookie.Path.Should().Be("/");

        var refreshCookie = refresh.GetRequiredCookie(AuthCookieNames.RefreshToken);
        refreshCookie.Value.Should().NotBeNullOrWhiteSpace();
        refreshCookie.HttpOnly.Should().BeTrue();
        refreshCookie.Path.Should().Be("/auth");
        refreshCookie.Value.Should().NotBe(registerRefreshCookie.Value);

        var currentUser = await authClient.GetAsync("/auth/current-user");
        currentUser.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_WithReusedRefreshCookie_ShouldRevokeTokenFamily()
    {
        var email = UniqueEmail("refresh-reuse");
        var initialClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));

        var register = await initialClient.RegisterAsync(email, "Str0ngPass!123", "Refresh Reuse User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var originalRefreshCookie = register.GetRequiredCookie(AuthCookieNames.RefreshToken);

        var firstRefresh = await initialClient.RefreshAsync();
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotatedRefreshCookie = firstRefresh.GetRequiredCookie(AuthCookieNames.RefreshToken);

        var reuseClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var secondRefresh = await reuseClient.RefreshWithRefreshTokenAsync(originalRefreshCookie.Value);
        secondRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var familyTokenAfterReuse = await reuseClient.RefreshWithRefreshTokenAsync(rotatedRefreshCookie.Value);
        familyTokenAfterReuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithoutRefreshCookie_ShouldReturnUnauthorized()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));

        var response = await authClient.RefreshAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
