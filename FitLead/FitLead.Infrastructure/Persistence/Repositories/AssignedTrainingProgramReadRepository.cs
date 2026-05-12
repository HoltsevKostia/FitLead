using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class AssignedTrainingProgramReadRepository
        : IAssignedTrainingProgramReadRepository
    {
        private readonly FitLeadDbContext _context;

        public AssignedTrainingProgramReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainingProgramAssignmentDto>> GetByProgramIdAsync(
            Guid trainingProgramId,
            CancellationToken cancellationToken)
        {
            return await (
                from assignment in _context.AssignedTrainingPrograms
                join client in _context.DomainUsers
                    on assignment.ClientId equals client.Id
                where assignment.TrainingProgramId == trainingProgramId
                orderby assignment.Status, assignment.AssignedAtUtc descending
                select new TrainingProgramAssignmentDto(
                    assignment.Id,
                    client.Id,
                    client.FullName,
                    assignment.Status.ToString(),
                    assignment.AccessSource.ToString(),
                    assignment.AssignedAtUtc,
                    assignment.ExpiresAtUtc,
                    assignment.RevokedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
