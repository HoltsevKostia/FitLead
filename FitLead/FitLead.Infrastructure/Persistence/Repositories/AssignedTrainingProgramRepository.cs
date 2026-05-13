using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class AssignedTrainingProgramRepository : IAssignedTrainingProgramRepository
    {
        private readonly FitLeadDbContext _context;

        public AssignedTrainingProgramRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            AssignedTrainingProgram assignment,
            CancellationToken cancellationToken)
        {
            await _context.AssignedTrainingPrograms.AddAsync(assignment, cancellationToken);
        }

        public async Task<AssignedTrainingProgram?> GetActiveByClientAndProgramAsync(
            Guid clientId,
            Guid trainingProgramId,
            CancellationToken cancellationToken)
        {
            return await _context.AssignedTrainingPrograms
                .FirstOrDefaultAsync(
                    x => x.ClientId == clientId &&
                         x.TrainingProgramId == trainingProgramId &&
                         x.Status == AssignedProgramStatus.Active,
                    cancellationToken);
        }

        public async Task<AssignedTrainingProgram?> GetByIdForProgramAndTrainerAsync(
            Guid assignmentId,
            Guid trainingProgramId,
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await _context.AssignedTrainingPrograms
                .FirstOrDefaultAsync(
                    x => x.Id == assignmentId &&
                         x.TrainingProgramId == trainingProgramId &&
                         x.TrainerId == trainerId,
                    cancellationToken);
        }
    }
}
