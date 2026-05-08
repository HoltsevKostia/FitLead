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
                var rows = await (
            from w in _context.Workouts.AsNoTracking()
            where w.Id == workoutId && w.TrainerId == trainerId

            join we0 in _context.WorkoutExercises.AsNoTracking()
                on w.Id equals EF.Property<Guid>(we0, "workout_id") into weGroup
            from we in weGroup.DefaultIfEmpty()

            join e0 in _context.Exercises.AsNoTracking()
                on we.ExerciseId equals e0.Id into eGroup
            from e in eGroup.DefaultIfEmpty()

            select new
            {
                WorkoutId = w.Id,
                w.TrainerId,
                WorkoutName = w.Name,

                WorkoutExerciseId = (Guid?)we.Id,
                ExerciseId = (Guid?)e.Id,
                ExerciseName = e != null ? e.Name : null,
                ExerciseDescription = e != null ? e.Description : null,
                ExerciseMediaUrl = e != null ? e.MediaUrl : null,

                Repetitions = (int?)we.Repetitions,
                Sets = (int?)we.Sets,
                RestSeconds = (int?)we.RestSeconds
            }
            ).ToListAsync(ct);

            if (rows.Count == 0)
                return null;

            var header = rows[0];

            var exercises = rows
                .Where(x => x.WorkoutExerciseId.HasValue)
                .Select(x => new WorkoutExerciseDetailsDto(
                    x.WorkoutExerciseId!.Value,
                    x.ExerciseId!.Value,
                    x.ExerciseName!,
                    x.ExerciseDescription!,
                    x.ExerciseMediaUrl?.Value,
                    x.Repetitions!.Value,
                    x.Sets!.Value,
                    x.RestSeconds!.Value
                ))
                .ToList();

            return new WorkoutDetailsDto(
                header.WorkoutId,
                header.TrainerId,
                header.WorkoutName,
                exercises
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
