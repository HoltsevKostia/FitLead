using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.WorkoutLogs.Access;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class WorkoutLogAccessRepository : IWorkoutLogAccessRepository
    {
        private readonly FitLeadDbContext _context;

        public WorkoutLogAccessRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<WorkoutLogAccessContext?> GetAccessibleForClientAsync(
            Guid assignmentId,
            Guid trainingProgramWorkoutId,
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            return await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join trainingProgramWorkout in _context.TrainingProgramWorkouts.AsNoTracking()
                        on assignment.TrainingProgramId equals trainingProgramWorkout.TrainingProgramId
                    where assignment.Id == assignmentId &&
                          assignment.ClientId == clientId &&
                          assignment.Status == AssignedProgramStatus.Active &&
                          (!assignment.ExpiresAtUtc.HasValue || assignment.ExpiresAtUtc > utcNow) &&
                          trainingProgramWorkout.Id == trainingProgramWorkoutId
                    select new WorkoutLogAccessContext(
                        assignment.Id,
                        trainingProgramWorkout.Id,
                        assignment.ClientId,
                        assignment.TrainerId))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
