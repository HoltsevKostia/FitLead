using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Trainings;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    internal sealed class WorkoutRepository : IWorkoutRepository
    {
        private readonly FitLeadDbContext _context;

        public WorkoutRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<Workout?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.Workouts
                .Include(x => x.Exercises)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(
            Workout workout,
            CancellationToken cancellationToken)
        {
            await _context.Workouts.AddAsync(workout, cancellationToken);
        }

        public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken)
        {
            return await _context.Workouts
                .AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Guid?> GetTrainerIdAsync(Guid workoutId, CancellationToken cancellationToken)
        {
            var workout = await _context.Workouts.FirstOrDefaultAsync(x => x.Id == workoutId, cancellationToken);
            
            return workout?.TrainerId;
        }

        public async Task DeleteTrainingProgramWorkoutsByWorkoutIdAsync(
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            await _context.TrainingProgramWorkouts
                .Where(x => x.WorkoutId == workoutId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public void Remove(Workout workout)
        {
            _context.Workouts.Remove(workout);
        }
    }
}
