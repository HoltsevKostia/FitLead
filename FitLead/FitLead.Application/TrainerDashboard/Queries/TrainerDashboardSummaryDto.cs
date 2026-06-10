namespace FitLead.Application.TrainerDashboard.Queries
{
    public sealed record TrainerDashboardSummaryDto(
        int ClientCount,
        int ActiveProgramAssignmentCount,
        int UnreadNotificationCount,
        int PendingVideoReportCount);
}
