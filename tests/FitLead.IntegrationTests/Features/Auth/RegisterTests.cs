using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class RegisterTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Register_WithValidTrainerPayload_ShouldReturnCreatedAndSetAuthCookies()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("trainer");

        var response = await authClient.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Test Trainer",
            AuthRoles.Trainer);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.ReadRequiredJsonAsync<AuthSessionResponse>();
        payload.ExpiresIn.Should().BePositive();

        var accessCookie = response.GetRequiredCookie(AuthCookieNames.AccessToken);
        accessCookie.Value.Should().NotBeNullOrWhiteSpace();
        accessCookie.HttpOnly.Should().BeTrue();
        accessCookie.Path.Should().Be("/");

        var refreshCookie = response.GetRequiredCookie(AuthCookieNames.RefreshToken);
        refreshCookie.Value.Should().NotBeNullOrWhiteSpace();
        refreshCookie.HttpOnly.Should().BeTrue();
        refreshCookie.Path.Should().Be("/auth");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflictWithAuthEmailExistsErrorCode()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("duplicate");

        var first = await authClient.RegisterAsync(
            email,
            "Str0ngPass!123",
            "First User",
            AuthRoles.Trainer);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await authClient.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Second User",
            AuthRoles.Trainer);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await second.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be(AuthErrorCodes.EmailExists);
        problem.Title.Should().Be("Conflict");
        problem.Detail.Should().Contain("already exists");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequestWithValidationProblem()
    {
        var authClient = new AuthTestClient(HttpClient);

        var response = await authClient.RegisterAsync(
            "not-an-email",
            "Str0ngPass!123",
            "Invalid Email User",
            AuthRoles.Trainer);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be(AuthErrorCodes.EmailInvalid);
        problem.Title.Should().Be("Validation");
        problem.Detail.Should().Contain("Email format is invalid");
    }

    [Fact]
    public async Task Register_WithValidPayload_ShouldAllowAccessToCurrentUserUsingIssuedCookies()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("register-session");

        var register = await authClient.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Registered User",
            AuthRoles.Trainer);

        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var currentUser = await HttpClient.GetAsync("/auth/current-user");
        currentUser.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
