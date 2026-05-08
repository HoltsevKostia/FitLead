using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class AuthorizationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task TrainerOnlyEndpoint_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        var response = await HttpClient.GetAsync("/api/workouts");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrainerOnlyEndpoint_WithClientRole_ShouldReturnForbidden()
    {
        var authClient = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("client-role");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Client User", AuthRoles.Client);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await authClient.GetAsync("/api/workouts");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
