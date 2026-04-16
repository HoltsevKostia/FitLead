using System.Net;
using FitLead.Api.Auth.Contracts;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class LoginTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithTokens()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("login");
        const string password = "Str0ngPass!123";

        var register = await authClient.RegisterAsync(email, password, "Login User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await authClient.LoginAsync(email, password);

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await login.ReadRequiredJsonAsync<LoginResponse>();
        payload.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
        payload.ExpiresIn.Should().BePositive();
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
