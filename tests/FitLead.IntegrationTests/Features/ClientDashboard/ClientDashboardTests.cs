using System.Net;
using FitLead.Application.ClientDashboard.Queries;
using FitLead.Domain.Trainings.WorkoutLogs;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.ClientDashboard;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ClientDashboardTests(IntegrationTestFixture fixture)
    : ClientDashboardTestBase(fixture)
{
    [Fact]
    public async Task Get_ShouldReturnTrainerAndActiveProgramsNewestFirst()
    {
        var trainer = await Users.RegisterTrainerAsync("client-dashboard-trainer");
        var client = await Users.RegisterClientAsync("client-dashboard-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var utcNow = DateTime.UtcNow;

        var olderProgram = await CreateProgramAsync(
            trainer.Id,
            client.Id,
            "Older active",
            utcNow.AddDays(-4),
            []);
        var newerProgram = await CreateProgramAsync(
            trainer.Id,
            client.Id,
            "Newer active",
            utcNow.AddDays(-2),
            []);
        await CreateProgramAsync(
            trainer.Id,
            client.Id,
            "Revoked",
            utcNow.AddDays(-5),
            [],
            revoke: true);
        await CreateProgramAsync(
            trainer.Id,
            client.Id,
            "Expired",
            utcNow.AddDays(-6),
            [],
            expiresAtUtc: utcNow.AddDays(-1));

        var dashboardClient = await Api.ClientDashboardAsync(client.Auth);
        var response = await dashboardClient.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.ReadRequiredJsonAsync<ClientDashboardDto>();
        dashboard.Trainer.Should().NotBeNull();
        dashboard.Trainer!.TrainerId.Should().Be(trainer.Id);
        dashboard.Trainer.FullName.Should().Be("Test Trainer");
        dashboard.ActivePrograms.Select(program => program.AssignmentId)
            .Should()
            .Equal(newerProgram.AssignmentId, olderProgram.AssignmentId);
    }

    [Fact]
    public async Task Get_ShouldChooseFirstPendingWorkoutByProgramOrder()
    {
        var trainer = await Users.RegisterTrainerAsync("client-dashboard-order-trainer");
        var client = await Users.RegisterClientAsync("client-dashboard-order-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);

        var program = await CreateProgramAsync(
            trainer.Id,
            client.Id,
            "Ordered program",
            DateTime.UtcNow.AddDays(-2),
            [
                ("Week two", 2, 1),
                ("Completed first", 1, 1),
                ("Skipped second", 1, 1),
                ("Next workout", 1, 2)
            ]);
        var orderedWorkouts = program.Workouts
            .OrderBy(workout => workout.WeekNumber)
            .ThenBy(workout => workout.DayNumber)
            .ThenBy(workout => workout.OrderInDay)
            .ToList();
        await CreateLogAsync(
            program,
            orderedWorkouts[0],
            trainer.Id,
            client.Id,
            WorkoutLogStatus.Completed);
        await CreateLogAsync(
            program,
            orderedWorkouts[1],
            trainer.Id,
            client.Id,
            WorkoutLogStatus.Skipped);

        var dashboardClient = await Api.ClientDashboardAsync(client.Auth);
        var response = await dashboardClient.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.ReadRequiredJsonAsync<ClientDashboardDto>();
        var summary = dashboard.ActivePrograms.Should().ContainSingle().Subject;
        summary.CompletedCount.Should().Be(1);
        summary.SkippedCount.Should().Be(1);
        summary.PendingCount.Should().Be(2);
        summary.NextWorkout.Should().NotBeNull();
        summary.NextWorkout!.ProgramWorkoutId.Should()
            .Be(orderedWorkouts[2].ProgramWorkoutId);
        summary.NextWorkout.WeekNumber.Should().Be(1);
        summary.NextWorkout.DayNumber.Should().Be(2);
    }

    [Fact]
    public async Task Get_WhenAllWorkoutsCompleted_ShouldReturnNullNextWorkout()
    {
        var trainer = await Users.RegisterTrainerAsync("client-dashboard-done-trainer");
        var client = await Users.RegisterClientAsync("client-dashboard-done-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var program = await CreateProgramAsync(
            trainer.Id,
            client.Id,
            "Finished program",
            DateTime.UtcNow.AddDays(-2),
            [("First completed", 1, 1), ("Second completed", 1, 2)]);
        await CreateLogAsync(
            program,
            program.Workouts[0],
            trainer.Id,
            client.Id,
            WorkoutLogStatus.Completed);
        await CreateLogAsync(
            program,
            program.Workouts[1],
            trainer.Id,
            client.Id,
            WorkoutLogStatus.Completed);

        var dashboardClient = await Api.ClientDashboardAsync(client.Auth);
        var response = await dashboardClient.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.ReadRequiredJsonAsync<ClientDashboardDto>();
        var summary = dashboard.ActivePrograms.Should().ContainSingle().Subject;
        summary.PendingCount.Should().Be(0);
        summary.NextWorkout.Should().BeNull();
    }

    [Fact]
    public async Task Get_AsTrainer_ShouldReturnForbidden()
    {
        var trainer = await Users.RegisterTrainerAsync("client-dashboard-forbidden");
        var dashboardClient = await Api.ClientDashboardAsync(trainer.Auth);

        var response = await dashboardClient.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_Unauthenticated_ShouldReturnUnauthorized()
    {
        using var anonymous = Fixture.CreateClient(handleCookies: false);

        var response = await anonymous.GetAsync("/api/client/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
