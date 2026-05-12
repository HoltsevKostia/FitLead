using System.Net;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FitLead.Infrastructure.Persistence.Models;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

public abstract class TrainingProgramTestBase : IntegrationTestBase
{
    protected TestUsers Users { get; }
    protected TestWorkouts Workouts { get; }
    protected TestApiClients Api { get; }
    protected TestDb Db { get; }

    protected TrainingProgramTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Users = new TestUsers(fixture, Db);
        Workouts = new TestWorkouts(Db);
        Api = new TestApiClients(fixture);
    }

    protected static async Task<Guid> CreateProgramAsync(
        TrainingProgramsTestClient client,
        string title = "Program",
        int weeksCount = 4,
        int daysPerWeek = 7)
    {
        var createResponse = await client.CreateAsync(title, weeksCount, daysPerWeek);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        return await createResponse.ReadRequiredJsonAsync<Guid>();
    }

    protected static async Task<IReadOnlyList<TrainingProgramWorkoutDto>> ReadProgramWorkoutsAsync(
        TrainingProgramsTestClient client,
        Guid programId)
    {
        var response = await client.GetWorkoutsAsync(programId);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadRequiredJsonAsync<IReadOnlyList<TrainingProgramWorkoutDto>>();
    }

    protected async Task CreateTrainerClientRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await Db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }
}
