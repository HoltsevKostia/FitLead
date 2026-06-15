using FitLead.Common.Results;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Trainings.Workouts;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Api.Seeding
{
    public static class SmokeDataSeeder
    {
        public const string ProgramTitle = "Smoke Test Program";
        public const string WorkoutName = "Smoke Test Workout";
        public const string VideoReportTitle = "Smoke Test Video Report";

        private const string TrainerEmail = "demo.trainer@fitlead.local";
        private const string ClientEmail = "demo.client@fitlead.local";
        private const string MediaStorageObjectId = "smoke-seed/video-report-image";

        public static async Task SeedAsync(
            IServiceProvider services,
            CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var nowUtc = DateTime.UtcNow;
            var trainer = await dbContext.DomainUsers
                .SingleAsync(user => user.Email == TrainerEmail, cancellationToken);
            var client = await dbContext.DomainUsers
                .SingleAsync(user => user.Email == ClientEmail, cancellationToken);
            var chat = await dbContext.Chats
                .SingleAsync(
                    candidate =>
                        candidate.TrainerId == trainer.Id &&
                        candidate.ClientId == client.Id,
                    cancellationToken);

            var exerciseId = await GetPlatformExerciseIdAsync(
                dbContext,
                cancellationToken);
            var workout = await EnsureWorkoutAsync(
                dbContext,
                trainer.Id,
                exerciseId,
                cancellationToken);
            var program = await EnsureProgramAsync(
                dbContext,
                trainer.Id,
                workout.Id,
                cancellationToken);

            await EnsureActiveAssignmentAsync(
                dbContext,
                trainer.Id,
                client.Id,
                program.Id,
                nowUtc,
                cancellationToken);
            await EnsurePendingVideoReportAsync(
                dbContext,
                chat,
                trainer.Id,
                client.Id,
                nowUtc,
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private static async Task<Guid> GetPlatformExerciseIdAsync(
            FitLeadDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var exerciseName = PlatformExerciseSeeder.Exercises[0].Name;

            return await dbContext.Exercises
                .Where(exercise => exercise.Name == exerciseName)
                .Select(exercise => exercise.Id)
                .SingleAsync(cancellationToken);
        }

        private static async Task<Workout> EnsureWorkoutAsync(
            FitLeadDbContext dbContext,
            Guid trainerId,
            Guid exerciseId,
            CancellationToken cancellationToken)
        {
            var workout = await dbContext.Workouts
                .Include(candidate => candidate.Exercises)
                .SingleOrDefaultAsync(
                    candidate =>
                        candidate.TrainerId == trainerId &&
                        candidate.Name == WorkoutName,
                    cancellationToken);

            if (workout is null)
            {
                workout = Require(Workout.Create(WorkoutName, trainerId));
                dbContext.Workouts.Add(workout);
            }

            if (workout.Exercises.All(entry => entry.ExerciseId != exerciseId))
            {
                RequireSuccess(
                    workout.AddExercise(
                        exerciseId,
                        repetitions: 10,
                        sets: 3,
                        loadKg: null,
                        restSeconds: 60,
                        trainerNote: "Keep the movement controlled."));
            }

            return workout;
        }

        private static async Task<TrainingProgram> EnsureProgramAsync(
            FitLeadDbContext dbContext,
            Guid trainerId,
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            var program = await dbContext.TrainingPrograms
                .Include(candidate => candidate.Workouts)
                .SingleOrDefaultAsync(
                    candidate =>
                        candidate.TrainerId == trainerId &&
                        candidate.Title == ProgramTitle,
                    cancellationToken);

            if (program is null)
            {
                program = Require(
                    TrainingProgram.Create(
                        trainerId,
                        ProgramTitle,
                        weeksCount: 1,
                        daysPerWeek: 1));
                dbContext.TrainingPrograms.Add(program);
            }

            if (program.Workouts.All(entry => entry.WorkoutId != workoutId))
            {
                RequireSuccess(
                    program.AddWorkout(
                        workoutId,
                        weekNumber: 1,
                        dayNumber: 1));
            }

            return program;
        }

        private static async Task EnsureActiveAssignmentAsync(
            FitLeadDbContext dbContext,
            Guid trainerId,
            Guid clientId,
            Guid programId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var activeAssignmentExists = await dbContext.AssignedTrainingPrograms
                .AnyAsync(
                    assignment =>
                        assignment.ClientId == clientId &&
                        assignment.TrainingProgramId == programId &&
                        assignment.Status == AssignedProgramStatus.Active &&
                        (!assignment.ExpiresAtUtc.HasValue ||
                         assignment.ExpiresAtUtc > nowUtc),
                    cancellationToken);
            if (activeAssignmentExists)
            {
                return;
            }

            dbContext.AssignedTrainingPrograms.Add(
                Require(
                    AssignedTrainingProgram.AssignManually(
                        trainerId,
                        clientId,
                        programId,
                        nowUtc)));
        }

        private static async Task EnsurePendingVideoReportAsync(
            FitLeadDbContext dbContext,
            Chat chat,
            Guid trainerId,
            Guid clientId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var report = await dbContext.VideoReports
                .SingleOrDefaultAsync(
                    candidate =>
                        candidate.ClientId == clientId &&
                        candidate.TrainerId == trainerId &&
                        candidate.Status == VideoReportStatus.Submitted &&
                        candidate.Title == VideoReportTitle,
                    cancellationToken);

            if (report is null)
            {
                var mediaAsset = await EnsureReportMediaAsync(
                    dbContext,
                    clientId,
                    nowUtc,
                    cancellationToken);
                var createdAtUtc = nowUtc.AddMinutes(-5);

                report = Require(
                    VideoReport.Create(
                        chat.Id,
                        clientId,
                        trainerId,
                        VideoReportTitle,
                        "Pending report created for the frontend smoke suite.",
                        [mediaAsset.Id],
                        createdAtUtc));
                dbContext.VideoReports.Add(report);
            }

            var messageExists = await dbContext.ChatMessages
                .AnyAsync(
                    message => message.VideoReportId == report.Id,
                    cancellationToken);
            if (messageExists)
            {
                return;
            }

            var messageCreatedAtUtc = report.CreatedAtUtc;
            dbContext.ChatMessages.Add(
                Require(
                    ChatMessage.CreateVideoReport(
                        chat,
                        report,
                        clientId,
                        messageCreatedAtUtc)));
            chat.MarkMessageCreated(messageCreatedAtUtc);
        }

        private static async Task<MediaAsset> EnsureReportMediaAsync(
            FitLeadDbContext dbContext,
            Guid clientId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var mediaAsset = await dbContext.MediaAssets
                .SingleOrDefaultAsync(
                    asset =>
                        asset.StorageProvider == MediaStorageProvider.LocalDev &&
                        asset.StorageObjectId == MediaStorageObjectId,
                    cancellationToken);
            if (mediaAsset is not null)
            {
                return mediaAsset;
            }

            mediaAsset = Require(
                MediaAsset.Create(
                    clientId,
                    MediaStorageProvider.LocalDev,
                    MediaStorageObjectId,
                    "http://localhost:3000/smoke/pulldown.jpg",
                    "pulldown.jpg",
                    "image/jpeg",
                    sizeBytes: 1024,
                    kind: MediaAssetKind.Image,
                    durationSeconds: null,
                    createdAtUtc: nowUtc.AddMinutes(-5)));
            dbContext.MediaAssets.Add(mediaAsset);

            return mediaAsset;
        }

        private static T Require<T>(Result<T> result)
        {
            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error.Message);
            }

            return result.Value;
        }

        private static void RequireSuccess(Result result)
        {
            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error.Message);
            }
        }
    }
}
