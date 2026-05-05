using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class LoginTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndSetAuthCookies()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("login");
        const string password = "Str0ngPass!123";

        var register = await authClient.RegisterAsync(email, password, "Login User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await authClient.LoginAsync(email, password);

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await login.ReadRequiredJsonAsync<AuthSessionResponse>();
        payload.ExpiresIn.Should().BePositive();

        var accessCookie = login.GetRequiredCookie(AuthCookieNames.AccessToken);
        accessCookie.HttpOnly.Should().BeTrue();
        accessCookie.Path.Should().Be("/");

        var refreshCookie = login.GetRequiredCookie(AuthCookieNames.RefreshToken);
        refreshCookie.HttpOnly.Should().BeTrue();
        refreshCookie.Path.Should().Be("/auth");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("invalid-login");
        const string password = "Str0ngPass!123";

        var register = await authClient.RegisterAsync(email, password, "Invalid Login User", AuthRoles.Client);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await authClient.LoginAsync(email, "wrong-password");

        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnUnauthorized()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("missing-user");

        var login = await authClient.LoginAsync(email, "Str0ngPass!123");

        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
