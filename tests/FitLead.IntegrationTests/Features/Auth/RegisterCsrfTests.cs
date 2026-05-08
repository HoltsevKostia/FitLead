using System.Net;
using System.Net.Http.Json;
using FitLead.Api.Auth.Contracts;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class RegisterCsrfTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Register_WithoutCsrfToken_ShouldBeRejected()
    {
        using var rawClient = Fixture.CreateClient(handleCookies: false);

        var response = await rawClient.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(
                UniqueEmail("register-csrf-missing"),
                "Str0ngPass!123",
                "Register Missing Csrf User",
                AuthRoles.Trainer));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithValidCsrfToken_AndValidPayload_ShouldFollowExistingRegisterContract()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));

        var response = await authClient.RegisterAsync(
            UniqueEmail("register-csrf-valid"),
            "Str0ngPass!123",
            "Register Valid Csrf User",
            AuthRoles.Trainer);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.GetRequiredCookie(AuthCookieNames.AccessToken).Value.Should().NotBeNullOrWhiteSpace();
        response.GetRequiredCookie(AuthCookieNames.RefreshToken).Value.Should().NotBeNullOrWhiteSpace();

        var currentUser = await authClient.GetAsync("/auth/current-user");
        currentUser.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithValidCsrfToken_AndInvalidPayload_ShouldReturnValidationOrBusinessError()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));

        var response = await authClient.RegisterAsync(
            "not-an-email",
            "Str0ngPass!123",
            "Register Invalid Email User",
            AuthRoles.Trainer);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be(AuthErrorCodes.EmailInvalid);
        problem.Title.Should().Be("Validation");
    }
}
