using System.Net;
using FitLead.Application.Clients.ClientProfiles;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.ClientProfiles;

public sealed class ClientProfileReadTests(IntegrationTestFixture fixture)
    : ClientProfileTestBase(fixture)
{
    [Fact]
    public async Task ClientCanGetEmptyDefaultProfile()
    {
        var client = await Users.RegisterClientAsync("client-profile-empty");
        var profiles = await Api.ClientProfilesAsync(client.Auth);

        var response = await profiles.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<ClientProfileDto>();
        dto.ClientId.Should().Be(client.Id);
        dto.Goal.Should().BeNull();
        dto.ExperienceLevel.Should().BeNull();
        dto.HeightCm.Should().BeNull();
        dto.Limitations.Should().BeNull();
        dto.TrainingPreferences.Should().BeNull();
        dto.AdditionalInfo.Should().BeNull();
        dto.CreatedAtUtc.Should().BeNull();
        dto.UpdatedAtUtc.Should().BeNull();
    }
}
