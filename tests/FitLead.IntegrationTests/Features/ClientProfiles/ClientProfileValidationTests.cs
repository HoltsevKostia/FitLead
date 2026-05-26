using System.Net;
using FitLead.Domain.Clients.ClientProfiles;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.ClientProfiles;

public sealed class ClientProfileValidationTests(IntegrationTestFixture fixture)
    : ClientProfileTestBase(fixture)
{
    [Theory]
    [InlineData(49)]
    [InlineData(301)]
    public async Task Update_WithInvalidHeight_ShouldReturnValidationError(int heightCm)
    {
        var client = await Users.RegisterClientAsync("client-profile-height");
        var profiles = await Api.ClientProfilesAsync(client.Auth);

        var response = await profiles.UpdateAsync(heightCm: heightCm);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("client_profile.create.height_out_of_range");
    }

    [Fact]
    public async Task Update_WithInvalidExperienceLevel_ShouldReturnValidationError()
    {
        var client = await Users.RegisterClientAsync("client-profile-level");
        var profiles = await Api.ClientProfilesAsync(client.Auth);

        var response = await profiles.UpdateAsync(experienceLevel: "Expert");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("client_profile.experience_level_invalid");
    }

    [Theory]
    [InlineData(TextField.Goal)]
    [InlineData(TextField.Limitations)]
    [InlineData(TextField.TrainingPreferences)]
    [InlineData(TextField.AdditionalInfo)]
    public async Task Update_WithTooLongText_ShouldReturnValidationError(TextField field)
    {
        var client = await Users.RegisterClientAsync($"client-profile-text-{field}");
        var profiles = await Api.ClientProfilesAsync(client.Auth);
        var longGoal = new string('a', ClientProfile.MaxGoalLength + 1);
        var longText = new string('a', ClientProfile.MaxLongTextLength + 1);

        var response = field switch
        {
            TextField.Goal => await profiles.UpdateAsync(goal: longGoal),
            TextField.Limitations => await profiles.UpdateAsync(limitations: longText),
            TextField.TrainingPreferences => await profiles.UpdateAsync(trainingPreferences: longText),
            TextField.AdditionalInfo => await profiles.UpdateAsync(additionalInfo: longText),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be(GetExpectedErrorCode(field));
    }

    private static string GetExpectedErrorCode(TextField field)
    {
        return field switch
        {
            TextField.Goal => "client_profile.create.goal_too_long",
            TextField.Limitations => "client_profile.create.limitations_too_long",
            TextField.TrainingPreferences => "client_profile.create.training_preferences_too_long",
            TextField.AdditionalInfo => "client_profile.create.additional_info_too_long",
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
    }

    public enum TextField
    {
        Goal,
        Limitations,
        TrainingPreferences,
        AdditionalInfo
    }
}
