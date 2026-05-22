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
                .AsNoTracking()
                .Where(x => x.TrainerId == trainerId)
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
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
            {
                return null;
            }

            var exercises = await (
                    from workoutExercise in _context.WorkoutExercises.AsNoTracking()
                    where workoutExercise.WorkoutId == workoutId
                    join exercise in _context.Exercises.AsNoTracking()
                        on workoutExercise.ExerciseId equals exercise.Id
                    join mediaAsset in _context.MediaAssets.AsNoTracking()
                        on exercise.MediaAssetId equals mediaAsset.Id into mediaAssets
                    from mediaAsset in mediaAssets.DefaultIfEmpty()
                    orderby workoutExercise.Order, workoutExercise.Id
                    select new
                    {
                        WorkoutExerciseId = workoutExercise.Id,
                        workoutExercise.ExerciseId,
                        workoutExercise.Order,
                        ExerciseName = exercise.Name,
                        ExerciseDescription = exercise.Description,
                        ExerciseMediaAsset = mediaAsset,
                        ExerciseMuscleGroup = exercise.MuscleGroup,
                        ExerciseEquipment = exercise.Equipment,
                        workoutExercise.Repetitions,
                        workoutExercise.Sets,
                        workoutExercise.LoadKg,
                        workoutExercise.RestSeconds,
                        workoutExercise.TrainerNote
                    })
                .ToListAsync(ct);

            var exerciseDtos = exercises
                .Select(x => new WorkoutExerciseDetailsDto(
                    x.WorkoutExerciseId,
                    x.ExerciseId,
                    x.Order,
                    x.ExerciseName,
                    x.ExerciseDescription,
                    MediaAssetProjectionMapper.ToPreviewDto(x.ExerciseMediaAsset),
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
