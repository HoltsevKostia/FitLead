using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class LogoutCsrfTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Logout_WithoutCsrfToken_ShouldBeRejected()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("logout-csrf-missing");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Logout Missing Csrf User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        using var rawClient = Fixture.CreateClient(handleCookies: false);
        var response = await rawClient.PostAsync("/auth/logout", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithValidCsrf_ShouldSucceedAndClearCookies()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("logout-csrf-valid");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Logout Valid Csrf User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await authClient.LogoutAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.GetRequiredCookie(AuthCookieNames.AccessToken).Value.Should().BeEmpty();
        response.GetRequiredCookie(AuthCookieNames.RefreshToken).Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Logout_WithValidCsrf_ShouldPreserveExistingIdempotentBehavior()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));

        var response = await authClient.LogoutAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
