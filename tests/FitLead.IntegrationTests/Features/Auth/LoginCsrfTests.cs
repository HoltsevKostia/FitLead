using System.Net;
using System.Net.Http.Json;
using FitLead.Api.Auth.Contracts;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class LoginCsrfTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Login_WithoutCsrfToken_ShouldBeRejected()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("login-csrf-missing");
        const string password = "Str0ngPass!123";

        var register = await authClient.RegisterAsync(email, password, "Login Missing Csrf User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        using var rawClient = Fixture.CreateClient(handleCookies: false);
        var response = await rawClient.PostAsJsonAsync("/auth/login", new LoginRequest(email, password));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCsrfToken_AndValidCredentials_ShouldReturnOkAndSetAuthCookies()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("login-csrf-valid");
        const string password = "Str0ngPass!123";

        var register = await authClient.RegisterAsync(email, password, "Login Valid Csrf User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await authClient.LoginAsync(email, password);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetRequiredCookie(AuthCookieNames.AccessToken).Value.Should().NotBeNullOrWhiteSpace();
        response.GetRequiredCookie(AuthCookieNames.RefreshToken).Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithValidCsrfToken_AndInvalidCredentials_ShouldFollowExistingLoginContract()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("login-csrf-invalid-credentials");
        const string password = "Str0ngPass!123";

        var register = await authClient.RegisterAsync(email, password, "Login Invalid Password User", AuthRoles.Client);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await authClient.LoginAsync(email, "wrong-password");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
