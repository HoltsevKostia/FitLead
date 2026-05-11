using System.Net;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramCreateTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task Create_WithValidProgramStructure_ShouldCreateProgram()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-create-valid");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);

        var response = await client.CreateAsync(
            title: "Beginner Full Body",
            weeksCount: 6,
            daysPerWeek: 3);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var programId = await response.ReadRequiredJsonAsync<Guid>();
        programId.Should().NotBeEmpty();

        var listResponse = await client.GetAsync();
        var programs = await listResponse.ReadRequiredJsonAsync<IReadOnlyList<TrainingProgramDto>>();
        var program = programs.Should().ContainSingle().Subject;
        program.Id.Should().Be(programId);
        program.Title.Should().Be("Beginner Full Body");
        program.WeeksCount.Should().Be(6);
        program.DaysPerWeek.Should().Be(3);
    }

    [Fact]
    public async Task Create_WithInvalidWeeksCount_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-create-weeks-invalid");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);

        var response = await client.CreateAsync(weeksCount: 25, daysPerWeek: 7);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.create.weeks_count_out_of_range");
    }

    [Fact]
    public async Task Create_WithInvalidDaysPerWeek_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-create-days-invalid");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);

        var response = await client.CreateAsync(weeksCount: 4, daysPerWeek: 8);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.create.days_per_week_out_of_range");
    }
}
