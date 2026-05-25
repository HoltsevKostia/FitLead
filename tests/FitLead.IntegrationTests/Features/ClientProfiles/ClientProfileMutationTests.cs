using System.Net;
using FitLead.Application.Clients.ClientProfiles;
using FitLead.Domain.Clients.ClientProfiles;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.ClientProfiles;

public sealed class ClientProfileMutationTests(IntegrationTestFixture fixture)
    : ClientProfileTestBase(fixture)
{
    [Fact]
    public async Task ClientCanCreateProfileThroughPut()
    {
        var client = await Users.RegisterClientAsync("client-profile-create");
        var profiles = await Api.ClientProfilesAsync(client.Auth);

        var response = await profiles.UpdateAsync(
            goal: "Build strength",
            experienceLevel: nameof(ClientExperienceLevel.Beginner),
            heightCm: 178,
            limitations: "Knee discomfort after running",
            trainingPreferences: "Gym three times per week",
            additionalInfo: "Has gym access");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<ClientProfileDto>();
        dto.ClientId.Should().Be(client.Id);
        dto.Goal.Should().Be("Build strength");
        dto.ExperienceLevel.Should().Be(nameof(ClientExperienceLevel.Beginner));
        dto.HeightCm.Should().Be(178);
        dto.Limitations.Should().Be("Knee discomfort after running");
        dto.TrainingPreferences.Should().Be("Gym three times per week");
        dto.AdditionalInfo.Should().Be("Has gym access");
        dto.CreatedAtUtc.Should().NotBeNull();
        dto.UpdatedAtUtc.Should().BeNull();

        var persisted = await Db.QueryAsync(context =>
            context.ClientProfiles.SingleAsync(x => x.ClientId == client.Id));
        persisted.Goal.Should().Be("Build strength");
        persisted.ExperienceLevel.Should().Be(ClientExperienceLevel.Beginner);
    }

    [Fact]
    public async Task ClientCanUpdateProfileThroughPut()
    {
        var client = await Users.RegisterClientAsync("client-profile-update");
        var profiles = await Api.ClientProfilesAsync(client.Auth);
        (await profiles.UpdateAsync(
            goal: "Initial goal",
            experienceLevel: nameof(ClientExperienceLevel.Beginner),
            heightCm: 170))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await profiles.UpdateAsync(
            goal: "Updated goal",
            experienceLevel: nameof(ClientExperienceLevel.Intermediate),
            heightCm: 171,
            limitations: "No running",
            trainingPreferences: "Prefer dumbbells",
            additionalInfo: "Sleeps better now");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<ClientProfileDto>();
        dto.Goal.Should().Be("Updated goal");
        dto.ExperienceLevel.Should().Be(nameof(ClientExperienceLevel.Intermediate));
        dto.HeightCm.Should().Be(171);
        dto.Limitations.Should().Be("No running");
        dto.TrainingPreferences.Should().Be("Prefer dumbbells");
        dto.AdditionalInfo.Should().Be("Sleeps better now");
        dto.UpdatedAtUtc.Should().NotBeNull();

        var profilesInDb = await Db.QueryAsync(context => context.ClientProfiles.ToListAsync());
        profilesInDb.Should().ContainSingle();
        profilesInDb.Single().ExperienceLevel.Should().Be(ClientExperienceLevel.Intermediate);
    }
}
