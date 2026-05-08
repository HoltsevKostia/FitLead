using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class RefreshCsrfTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Refresh_WithoutCsrfToken_ShouldBeRejected()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("refresh-csrf-missing");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Refresh Missing Csrf User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        using var rawClient = Fixture.CreateClient(handleCookies: false);
        var response = await rawClient.PostAsync("/auth/refresh", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Refresh_WithValidCsrf_AndValidRefreshCookie_ShouldSucceed()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("refresh-csrf-valid");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Refresh Valid Csrf User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var originalRefreshCookie = register.GetRequiredCookie(AuthCookieNames.RefreshToken);

        var response = await authClient.RefreshAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetRequiredCookie(AuthCookieNames.AccessToken).Value.Should().NotBeNullOrWhiteSpace();

        var rotatedRefreshCookie = response.GetRequiredCookie(AuthCookieNames.RefreshToken);
        rotatedRefreshCookie.Value.Should().NotBeNullOrWhiteSpace();
        rotatedRefreshCookie.Value.Should().NotBe(originalRefreshCookie.Value);
    }

    [Fact]
    public async Task Refresh_WithValidCsrf_ButMissingRefreshCookie_ShouldFollowExistingAuthContract()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));

        var response = await authClient.RefreshAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
