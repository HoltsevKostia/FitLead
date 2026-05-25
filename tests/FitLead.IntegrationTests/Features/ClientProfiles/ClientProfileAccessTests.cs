using System.Net;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.ClientProfiles;

public sealed class ClientProfileAccessTests(IntegrationTestFixture fixture)
    : ClientProfileTestBase(fixture)
{
    [Fact]
    public async Task UnauthenticatedUserCannotAccessProfile()
    {
        using var anonymous = Fixture.CreateClient(handleCookies: false);

        var response = await anonymous.GetAsync("/api/client/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrainerCannotMutateClientProfile()
    {
        var trainer = await Users.RegisterTrainerAsync("client-profile-trainer-mutates");
        var profiles = await Api.ClientProfilesAsync(trainer.Auth);

        var response = await profiles.UpdateAsync(goal: "Trainer should not update this endpoint");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ClientUpdateWithoutCsrf_ShouldReturnBadRequest()
    {
        var client = await Users.RegisterClientAsync("client-profile-csrf");
        var profiles = await Api.ClientProfilesAsync(client.Auth);

        var response = await profiles.UpdateAsync(
            goal: "No CSRF",
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
