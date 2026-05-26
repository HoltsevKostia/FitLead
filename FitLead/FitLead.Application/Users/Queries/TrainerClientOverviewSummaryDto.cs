using FitLead.Application.Clients.BodyMetrics;
using FitLead.Application.Clients.ProgressPhotos;

namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientOverviewSummaryDto(
        TrainerClientActiveProgramSummaryDto? ActiveProgram,
        TrainerClientWorkoutLogCountsDto WorkoutLogCounts,
        TrainerClientLastWorkoutLogDto? LastWorkoutLog,
        TrainerClientLastVideoReportDto? LastVideoReport,
        ClientBodyMetricEntryDto? LastBodyMetric,
        ClientProgressPhotoDto? LastProgressPhoto);
}
