using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Clients.BodyMetrics;
using FitLead.Application.Clients.ProgressPhotos;
using FitLead.Application.Users.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.WorkoutLogs;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientOverviewReadRepository : ITrainerClientOverviewReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientOverviewReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<TrainerClientOverviewSummaryDto> GetOverviewSummaryAsync(
            Guid trainerId,
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            var activeProgram = await GetActiveProgramAsync(
                trainerId,
                clientId,
                utcNow,
                cancellationToken);

            var counts = activeProgram is null
                ? new TrainerClientWorkoutLogCountsDto(0, 0, 0)
                : await GetWorkoutLogCountsAsync(
                    activeProgram.AssignmentId,
                    activeProgram.TotalWorkouts,
                    cancellationToken);

            var lastWorkoutLog = await GetLastWorkoutLogAsync(
                trainerId,
                clientId,
                cancellationToken);

            var lastVideoReport = await GetLastVideoReportAsync(
                trainerId,
                clientId,
                cancellationToken);

            var lastBodyMetric = await GetLastBodyMetricAsync(
                clientId,
                cancellationToken);

            var lastProgressPhoto = await GetLastProgressPhotoAsync(
                clientId,
                cancellationToken);

            return new TrainerClientOverviewSummaryDto(
                activeProgram,
                counts,
                lastWorkoutLog,
                lastVideoReport,
                lastBodyMetric,
                lastProgressPhoto);
        }

        private async Task<TrainerClientActiveProgramSummaryDto?> GetActiveProgramAsync(
            Guid trainerId,
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            var activeProgram = await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join program in _context.TrainingPrograms.AsNoTracking()
                        on assignment.TrainingProgramId equals program.Id
                    where assignment.TrainerId == trainerId &&
                          assignment.ClientId == clientId &&
                          assignment.Status == AssignedProgramStatus.Active &&
                          (!assignment.ExpiresAtUtc.HasValue || assignment.ExpiresAtUtc > utcNow)
                    orderby assignment.AssignedAtUtc descending, assignment.Id descending
                    select new
                    {
                        AssignmentId = assignment.Id,
                        ProgramId = program.Id,
                        ProgramTitle = program.Title,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc
                    })
                .FirstOrDefaultAsync(cancellationToken);

            if (activeProgram is null)
            {
                return null;
            }

            var totalWorkouts = await _context.TrainingProgramWorkouts
                .AsNoTracking()
                .CountAsync(
                    workout => workout.TrainingProgramId == activeProgram.ProgramId,
                    cancellationToken);

            return new TrainerClientActiveProgramSummaryDto(
                activeProgram.AssignmentId,
                activeProgram.ProgramId,
                activeProgram.ProgramTitle,
                activeProgram.AssignedAtUtc,
                activeProgram.ExpiresAtUtc,
                totalWorkouts);
        }

        private async Task<TrainerClientWorkoutLogCountsDto> GetWorkoutLogCountsAsync(
            Guid assignmentId,
            int totalWorkouts,
            CancellationToken cancellationToken)
        {
            var logs = await _context.WorkoutLogs
                .AsNoTracking()
                .Where(log => log.AssignedTrainingProgramId == assignmentId)
                .GroupBy(log => log.Status)
                .Select(group => new
                {
                    Status = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);

            var completed = logs
                .Where(log => log.Status == WorkoutLogStatus.Completed)
                .Sum(log => log.Count);
            var skipped = logs
                .Where(log => log.Status == WorkoutLogStatus.Skipped)
                .Sum(log => log.Count);
            var pending = Math.Max(0, totalWorkouts - completed - skipped);

            return new TrainerClientWorkoutLogCountsDto(completed, skipped, pending);
        }

        private async Task<TrainerClientLastWorkoutLogDto?> GetLastWorkoutLogAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var log = await (
                    from workoutLog in _context.WorkoutLogs.AsNoTracking()
                    join assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                        on workoutLog.AssignedTrainingProgramId equals assignment.Id
                    join programWorkout in _context.TrainingProgramWorkouts.AsNoTracking()
                        on workoutLog.TrainingProgramWorkoutId equals programWorkout.Id
                    join program in _context.TrainingPrograms.AsNoTracking()
                        on assignment.TrainingProgramId equals program.Id
                    join workout in _context.Workouts.AsNoTracking()
                        on programWorkout.WorkoutId equals workout.Id
                    where workoutLog.TrainerId == trainerId &&
                          workoutLog.ClientId == clientId
                    orderby (workoutLog.UpdatedAtUtc ?? workoutLog.CreatedAtUtc) descending,
                        workoutLog.Id descending
                    select new
                    {
                        workoutLog.Id,
                        AssignmentId = assignment.Id,
                        ProgramWorkoutId = programWorkout.Id,
                        ProgramTitle = program.Title,
                        WorkoutName = workout.Name,
                        programWorkout.WeekNumber,
                        programWorkout.DayNumber,
                        programWorkout.OrderInDay,
                        workoutLog.Status,
                        workoutLog.PerformedAtUtc,
                        workoutLog.ClientNote,
                        workoutLog.DifficultyRating,
                        workoutLog.CreatedAtUtc,
                        workoutLog.UpdatedAtUtc
                    })
                .FirstOrDefaultAsync(cancellationToken);

            return log is null
                ? null
                : new TrainerClientLastWorkoutLogDto(
                    log.Id,
                    log.AssignmentId,
                    log.ProgramWorkoutId,
                    log.ProgramTitle,
                    log.WorkoutName,
                    log.WeekNumber,
                    log.DayNumber,
                    log.OrderInDay,
                    log.Status.ToString(),
                    log.PerformedAtUtc,
                    log.ClientNote,
                    log.DifficultyRating,
                    log.CreatedAtUtc,
                    log.UpdatedAtUtc);
        }

        private async Task<TrainerClientLastVideoReportDto?> GetLastVideoReportAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var report = await _context.VideoReports
                .AsNoTracking()
                .Where(videoReport =>
                    videoReport.TrainerId == trainerId &&
                    videoReport.ClientId == clientId)
                .OrderByDescending(videoReport => videoReport.CreatedAtUtc)
                .ThenByDescending(videoReport => videoReport.Id)
                .Select(videoReport => new
                {
                    ReportId = videoReport.Id,
                    videoReport.ChatId,
                    videoReport.Title,
                    videoReport.Description,
                    videoReport.Status,
                    MediaCount = _context.VideoReportMedia.Count(media =>
                        media.VideoReportId == videoReport.Id),
                    videoReport.CreatedAtUtc,
                    videoReport.ReviewedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            return report is null
                ? null
                : new TrainerClientLastVideoReportDto(
                    report.ReportId,
                    report.ChatId,
                    report.Title,
                    report.Description,
                    report.Status.ToString(),
                    report.MediaCount,
                    report.CreatedAtUtc,
                    report.ReviewedAtUtc);
        }

        private async Task<ClientBodyMetricEntryDto?> GetLastBodyMetricAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.ClientBodyMetricEntries
                .AsNoTracking()
                .Where(entry => entry.ClientId == clientId)
                .OrderByDescending(entry => entry.RecordedAt)
                .ThenByDescending(entry => entry.CreatedAtUtc)
                .ThenByDescending(entry => entry.Id)
                .Select(entry => new ClientBodyMetricEntryDto(
                    entry.Id,
                    entry.ClientId,
                    entry.RecordedAt,
                    entry.WeightKg,
                    entry.BodyFatPercent,
                    entry.ChestCm,
                    entry.WaistCm,
                    entry.HipsCm,
                    entry.ArmCm,
                    entry.ThighCm,
                    entry.Note,
                    entry.CreatedAtUtc,
                    entry.UpdatedAtUtc))
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<ClientProgressPhotoDto?> GetLastProgressPhotoAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var photo = await _context.ClientProgressPhotos
                .AsNoTracking()
                .Where(progressPhoto => progressPhoto.ClientId == clientId)
                .Join(
                    _context.MediaAssets.AsNoTracking(),
                    progressPhoto => progressPhoto.MediaAssetId,
                    mediaAsset => mediaAsset.Id,
                    (progressPhoto, mediaAsset) => new
                    {
                        Photo = progressPhoto,
                        MediaAsset = mediaAsset
                    })
                .OrderByDescending(item => item.Photo.TakenAt)
                .ThenByDescending(item => item.Photo.CreatedAtUtc)
                .ThenByDescending(item => item.Photo.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return photo is null
                ? null
                : new ClientProgressPhotoDto(
                    photo.Photo.Id,
                    photo.Photo.ClientId,
                    photo.Photo.MediaAssetId,
                    MediaAssetProjectionMapper.ToPreviewDto(photo.MediaAsset)!,
                    photo.Photo.TakenAt,
                    photo.Photo.Label.ToString(),
                    photo.Photo.Note,
                    photo.Photo.CreatedAtUtc);
        }
    }
}
