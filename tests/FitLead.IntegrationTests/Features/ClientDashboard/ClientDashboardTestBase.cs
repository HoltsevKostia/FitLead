using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Trainings.WorkoutLogs;
using FitLead.Domain.Trainings.Workouts;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.ClientDashboard;

public abstract class ClientDashboardTestBase(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    protected readonly TestDb Db = new(fixture);
    protected readonly TestUsers Users = new(fixture, new TestDb(fixture));
    protected readonly TestApiClients Api = new(fixture);

    protected async Task CreateRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await Db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }

    protected async Task<DashboardProgramSetup> CreateProgramAsync(
        Guid trainerId,
        Guid clientId,
        string title,
        DateTime assignedAtUtc,
        IReadOnlyList<(string Name, int Week, int Day)> workoutSlots,
        DateTime? expiresAtUtc = null,
        bool revoke = false)
    {
        var programResult = TrainingProgram.Create(
            trainerId,
            title,
            weeksCount: 4,
            daysPerWeek: 7);
        programResult.IsSuccess.Should().BeTrue();

        var workouts = workoutSlots
            .Select(slot => Workout.Create(slot.Name, trainerId))
            .ToList();
        foreach (var workout in workouts)
        {
            workout.IsSuccess.Should().BeTrue();
        }

        for (var index = 0; index < workoutSlots.Count; index++)
        {
            programResult.Value.AddWorkout(
                workouts[index].Value.Id,
                workoutSlots[index].Week,
                workoutSlots[index].Day).IsSuccess.Should().BeTrue();
        }

        var assignmentResult = AssignedTrainingProgram.AssignManually(
            trainerId,
            clientId,
            programResult.Value.Id,
            assignedAtUtc,
            expiresAtUtc);
        assignmentResult.IsSuccess.Should().BeTrue();

        if (revoke)
        {
            assignmentResult.Value.Revoke(assignedAtUtc.AddMinutes(1))
                .IsSuccess.Should().BeTrue();
        }

        await Db.ExecuteAsync(async context =>
        {
            await context.Workouts.AddRangeAsync(workouts.Select(result => result.Value));
            await context.TrainingPrograms.AddAsync(programResult.Value);
            await context.AssignedTrainingPrograms.AddAsync(assignmentResult.Value);
            await context.SaveChangesAsync();
        });

        var entries = programResult.Value.Workouts
            .OrderBy(entry => entry.WeekNumber)
            .ThenBy(entry => entry.DayNumber)
            .ThenBy(entry => entry.OrderInDay)
            .Select(entry => new DashboardWorkoutEntry(
                entry.Id,
                entry.WorkoutId,
                entry.WeekNumber,
                entry.DayNumber,
                entry.OrderInDay))
            .ToList();

        return new DashboardProgramSetup(
            programResult.Value.Id,
            assignmentResult.Value.Id,
            entries);
    }

    protected async Task CreateLogAsync(
        DashboardProgramSetup program,
        DashboardWorkoutEntry workout,
        Guid trainerId,
        Guid clientId,
        WorkoutLogStatus status)
    {
        var utcNow = DateTime.UtcNow.AddHours(-1);
        var logResult = status switch
        {
            WorkoutLogStatus.Completed => WorkoutLog.CreateCompleted(
                program.AssignmentId,
                workout.ProgramWorkoutId,
                clientId,
                trainerId,
                utcNow,
                clientNote: null,
                difficultyRating: null,
                utcNow),
            WorkoutLogStatus.Skipped => WorkoutLog.CreateSkipped(
                program.AssignmentId,
                workout.ProgramWorkoutId,
                clientId,
                trainerId,
                clientNote: null,
                utcNow),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
        logResult.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.WorkoutLogs.AddAsync(logResult.Value);
            await context.SaveChangesAsync();
        });
    }

    protected sealed record DashboardProgramSetup(
        Guid ProgramId,
        Guid AssignmentId,
        IReadOnlyList<DashboardWorkoutEntry> Workouts);

    protected sealed record DashboardWorkoutEntry(
        Guid ProgramWorkoutId,
        Guid WorkoutId,
        int WeekNumber,
        int DayNumber,
        int OrderInDay);
}
