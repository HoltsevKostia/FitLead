using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Trainings.WorkoutLogs;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class WorkoutLogRepository : IWorkoutLogRepository
    {
        private readonly FitLeadDbContext _context;

        public WorkoutLogRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(WorkoutLog workoutLog, CancellationToken cancellationToken)
        {
            await _context.WorkoutLogs.AddAsync(workoutLog, cancellationToken);
        }

        public async Task<WorkoutLog?> GetByAssignmentWorkoutAsync(
            Guid assignedTrainingProgramId,
            Guid trainingProgramWorkoutId,
            CancellationToken cancellationToken)
        {
            return await _context.WorkoutLogs
                .FirstOrDefaultAsync(
                    x => x.AssignedTrainingProgramId == assignedTrainingProgramId &&
                         x.TrainingProgramWorkoutId == trainingProgramWorkoutId,
                    cancellationToken);
        }
    }
}
