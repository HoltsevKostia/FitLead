using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Workouts.Queries;
using Microsoft.EntityFrameworkCore;


namespace FitLead.Infrastructure.Persistence.Repositories
{
    internal sealed class WorkoutReadRepository : IWorkoutReadRepository
    {
        private readonly FitLeadDbContext _context;

        public WorkoutReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<WorkoutDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await _context.Workouts
                .Where(x => x.TrainerId == trainerId)
                .Select(x => new WorkoutDto(
                    x.Id,
                    x.Name,
                    x.TrainerId))
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkoutDetailsDto?> GetWorkoutDetailsByIdAsync(
            Guid workoutId,
            Guid trainerId,
            CancellationToken ct)
        {
            var workout = await _context.Workouts
                .AsNoTracking()
                .Where(x => x.Id == workoutId && x.TrainerId == trainerId)
                .Select(x => new
                {
                    x.Id,
                    x.TrainerId,
                    x.Name
                })
                .FirstOrDefaultAsync(ct);

            if (workout is null)
                return null;

            var exercises = await (
                from we in _context.WorkoutExercises.AsNoTracking()
                where we.WorkoutId == workoutId
                join e in _context.Exercises.AsNoTracking()
                    on we.ExerciseId equals e.Id
                orderby we.Order
                select new
                {
                    WorkoutExerciseId = we.Id,
                    we.ExerciseId,
                    we.Order,
                    ExerciseName = e.Name,
                    ExerciseDescription = e.Description,
                    ExerciseMediaUrl = e.MediaUrl,
                    ExerciseMuscleGroup = e.MuscleGroup,
                    ExerciseEquipment = e.Equipment,
                    we.Repetitions,
                    we.Sets,
                    we.LoadKg,
                    we.RestSeconds,
                    we.TrainerNote
                })
                .ToListAsync(ct);

            var exerciseDtos = exercises
                .Select(x => new WorkoutExerciseDetailsDto(
                    x.WorkoutExerciseId,
                    x.ExerciseId,
                    x.Order,
                    x.ExerciseName,
                    x.ExerciseDescription,
                    x.ExerciseMediaUrl?.Value,
                    x.ExerciseMuscleGroup,
                    x.ExerciseEquipment,
                    x.Repetitions,
                    x.Sets,
                    x.LoadKg,
                    x.RestSeconds,
                    x.TrainerNote
                ))
                .ToList();

            return new WorkoutDetailsDto(
                workout.Id,
                workout.TrainerId,
                workout.Name,
                exerciseDtos
            );
        }

        public async Task<int> GetUsageCountAsync(
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            return await _context.TrainingProgramWorkouts
                .CountAsync(x => x.WorkoutId == workoutId, cancellationToken);
        }
    }
}
