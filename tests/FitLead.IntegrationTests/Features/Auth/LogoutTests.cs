using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class LogoutTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Logout_WithAuthenticatedSession_ShouldClearAuthCookiesAndInvalidateCurrentUser()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("logout");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Logout User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var logout = await authClient.LogoutAsync();
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var accessCookie = logout.GetRequiredCookie(AuthCookieNames.AccessToken);
        accessCookie.Value.Should().BeEmpty();
        accessCookie.Path.Should().Be("/");

        var refreshCookie = logout.GetRequiredCookie(AuthCookieNames.RefreshToken);
        refreshCookie.Value.Should().BeEmpty();
        refreshCookie.Path.Should().Be("/auth");

        var currentUser = await HttpClient.GetAsync("/auth/current-user");
        currentUser.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
