using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.TrainerDashboard.Queries;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerDashboardReadRepository : ITrainerDashboardReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerDashboardReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<TrainerDashboardSummaryDto> GetSummaryAsync(
            Guid trainerId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            var clientCount = await _context.TrainerClients
                .AsNoTracking()
                .CountAsync(
                    relationship => relationship.TrainerId == trainerId,
                    cancellationToken);

            var activeProgramAssignmentCount = await _context.AssignedTrainingPrograms
                .AsNoTracking()
                .CountAsync(
                    assignment =>
                        assignment.TrainerId == trainerId &&
                        assignment.Status == AssignedProgramStatus.Active &&
                        (!assignment.ExpiresAtUtc.HasValue || assignment.ExpiresAtUtc > utcNow),
                    cancellationToken);

            var unreadNotificationCount = await _context.Notifications
                .AsNoTracking()
                .CountAsync(
                    notification =>
                        notification.RecipientUserId == trainerId &&
                        !notification.IsRead,
                    cancellationToken);

            var pendingVideoReportCount = await _context.VideoReports
                .AsNoTracking()
                .CountAsync(
                    report =>
                        report.TrainerId == trainerId &&
                        report.Status == VideoReportStatus.Submitted,
                    cancellationToken);

            return new TrainerDashboardSummaryDto(
                clientCount,
                activeProgramAssignmentCount,
                unreadNotificationCount,
                pendingVideoReportCount);
        }
    }
}
