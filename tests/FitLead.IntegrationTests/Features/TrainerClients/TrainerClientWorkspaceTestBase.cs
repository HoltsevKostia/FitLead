using FitLead.Domain.Clients.BodyMetrics;
using FitLead.Domain.Clients.ClientProfiles;
using FitLead.Domain.Clients.ProgressPhotos;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Trainings.WorkoutLogs;
using FitLead.Domain.Trainings.Workouts;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainerClients;

public abstract class TrainerClientWorkspaceTestBase(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    protected readonly TestDb Db = new(fixture);
    protected readonly TestUsers Users = new(fixture, new TestDb(fixture));
    protected readonly TestApiClients Api = new(fixture);

    protected async Task CreateTrainerClientRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await Db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }

    protected async Task<(Guid ProgramId, Guid AssignmentId, Guid ProgramWorkoutId, Guid WorkoutId)> CreateAssignedWorkoutAsync(
        Guid trainerId,
        Guid clientId,
        string programTitle = "Strength Base",
        string workoutName = "Full Body")
    {
        var workoutResult = Workout.Create(workoutName, trainerId);
        workoutResult.IsSuccess.Should().BeTrue();

        var programResult = TrainingProgram.Create(trainerId, programTitle, weeksCount: 4, daysPerWeek: 3);
        programResult.IsSuccess.Should().BeTrue();
        programResult.Value.AddWorkout(workoutResult.Value.Id, weekNumber: 1, dayNumber: 2)
            .IsSuccess.Should().BeTrue();

        var assignmentResult = AssignedTrainingProgram.AssignManually(
            trainerId,
            clientId,
            programResult.Value.Id,
            DateTime.UtcNow.AddDays(-2));
        assignmentResult.IsSuccess.Should().BeTrue();

        var programWorkoutId = programResult.Value.Workouts.Single().Id;

        await Db.ExecuteAsync(async context =>
        {
            await context.Workouts.AddAsync(workoutResult.Value);
            await context.TrainingPrograms.AddAsync(programResult.Value);
            await context.AssignedTrainingPrograms.AddAsync(assignmentResult.Value);
            await context.SaveChangesAsync();
        });

        return (
            programResult.Value.Id,
            assignmentResult.Value.Id,
            programWorkoutId,
            workoutResult.Value.Id);
    }

    protected async Task<Guid> CreateCompletedWorkoutLogAsync(
        Guid trainerId,
        Guid clientId,
        Guid assignmentId,
        Guid programWorkoutId,
        string note = "Felt strong today")
    {
        var logResult = WorkoutLog.CreateCompleted(
            assignmentId,
            programWorkoutId,
            clientId,
            trainerId,
            DateTime.UtcNow.AddDays(-1),
            note,
            difficultyRating: 8,
            DateTime.UtcNow.AddDays(-1));
        logResult.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.WorkoutLogs.AddAsync(logResult.Value);
            await context.SaveChangesAsync();
        });

        return logResult.Value.Id;
    }

    protected async Task<Guid> CreateBodyMetricAsync(Guid clientId)
    {
        var metricResult = ClientBodyMetricEntry.Create(
            clientId,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            weightKg: 78.5m,
            bodyFatPercent: 18.2m,
            chestCm: null,
            waistCm: 84m,
            hipsCm: null,
            armCm: null,
            thighCm: null,
            note: "First checkpoint",
            DateTime.UtcNow.AddDays(-1));
        metricResult.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.ClientBodyMetricEntries.AddAsync(metricResult.Value);
            await context.SaveChangesAsync();
        });

        return metricResult.Value.Id;
    }

    protected async Task<Guid> CreateProgressPhotoAsync(Guid clientId)
    {
        var mediaResult = MediaAsset.Create(
            clientId,
            MediaStorageProvider.LocalDev,
            $"progress/{Guid.NewGuid():N}.jpg",
            "http://localhost/media/progress.jpg",
            "progress.jpg",
            "image/jpeg",
            sizeBytes: 1024,
            MediaAssetKind.Image,
            durationSeconds: null,
            DateTime.UtcNow.AddDays(-1));
        mediaResult.IsSuccess.Should().BeTrue();

        var photoResult = ClientProgressPhoto.Create(
            clientId,
            mediaResult.Value.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            ProgressPhotoLabel.Front,
            "Front view",
            DateTime.UtcNow.AddDays(-1));
        photoResult.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.MediaAssets.AddAsync(mediaResult.Value);
            await context.ClientProgressPhotos.AddAsync(photoResult.Value);
            await context.SaveChangesAsync();
        });

        return photoResult.Value.Id;
    }

    protected async Task<Guid> CreateClientProfileAsync(Guid clientId)
    {
        var profileResult = ClientProfile.Create(
            clientId,
            "Build strength",
            ClientExperienceLevel.Intermediate,
            heightCm: 178,
            limitations: "Knee pain after running",
            trainingPreferences: "Gym, 3 days per week",
            additionalInfo: "Has gym access",
            DateTime.UtcNow.AddDays(-1));
        profileResult.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.ClientProfiles.AddAsync(profileResult.Value);
            await context.SaveChangesAsync();
        });

        return profileResult.Value.Id;
    }

    protected async Task<Guid> CreateVideoReportAsync(Guid trainerId, Guid clientId)
    {
        var chatResult = Chat.Create(trainerId, clientId, DateTime.UtcNow.AddDays(-2));
        chatResult.IsSuccess.Should().BeTrue();

        var mediaResult = MediaAsset.Create(
            clientId,
            MediaStorageProvider.LocalDev,
            $"reports/{Guid.NewGuid():N}.mp4",
            "http://localhost/media/report.mp4",
            "report.mp4",
            "video/mp4",
            sizeBytes: 2048,
            MediaAssetKind.Video,
            durationSeconds: 30,
            DateTime.UtcNow.AddDays(-1));
        mediaResult.IsSuccess.Should().BeTrue();

        var reportResult = VideoReport.Create(
            chatResult.Value.Id,
            clientId,
            trainerId,
            "Squat check",
            "Please review my squat",
            [mediaResult.Value.Id],
            DateTime.UtcNow.AddDays(-1));
        reportResult.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.Chats.AddAsync(chatResult.Value);
            await context.MediaAssets.AddAsync(mediaResult.Value);
            await context.VideoReports.AddAsync(reportResult.Value);
            await context.SaveChangesAsync();
        });

        return reportResult.Value.Id;
    }
}
