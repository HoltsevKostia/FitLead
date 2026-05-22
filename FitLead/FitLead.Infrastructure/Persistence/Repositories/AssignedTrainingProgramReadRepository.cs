using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;
using FitLead.Application.Trainings.Workouts.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
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
            var assignments = await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join client in _context.DomainUsers.AsNoTracking()
                        on assignment.ClientId equals client.Id
                    where assignment.TrainingProgramId == trainingProgramId
                    orderby assignment.Status, assignment.AssignedAtUtc descending, assignment.Id
                    select new
                    {
                        assignment.Id,
                        ClientId = client.Id,
                        ClientName = client.FullName,
                        assignment.Status,
                        assignment.AccessSource,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc,
                        assignment.RevokedAtUtc
                    })
                .ToListAsync(cancellationToken);

            return assignments
                .Select(assignment => new TrainingProgramAssignmentDto(
                    assignment.Id,
                    assignment.ClientId,
                    assignment.ClientName,
                    assignment.Status.ToString(),
                    assignment.AccessSource.ToString(),
                    assignment.AssignedAtUtc,
                    assignment.ExpiresAtUtc,
                    assignment.RevokedAtUtc))
                .ToList();
        }

        public async Task<IReadOnlyList<ClientAssignedTrainingProgramDto>> GetAccessibleByClientIdAsync(
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            return await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join program in _context.TrainingPrograms.AsNoTracking()
                        on assignment.TrainingProgramId equals program.Id
                    join trainer in _context.DomainUsers.AsNoTracking()
                        on assignment.TrainerId equals trainer.Id
                    where assignment.ClientId == clientId &&
                          assignment.Status == AssignedProgramStatus.Active &&
                          (!assignment.ExpiresAtUtc.HasValue || assignment.ExpiresAtUtc > utcNow)
                    orderby assignment.AssignedAtUtc descending, assignment.Id
                    select new ClientAssignedTrainingProgramDto(
                        assignment.Id,
                        program.Id,
                        program.Title,
                        trainer.Id,
                        trainer.FullName,
                        program.WeeksCount,
                        program.DaysPerWeek,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc))
                .ToListAsync(cancellationToken);
        }

        public async Task<ClientAssignedTrainingProgramDetailsDto?> GetAccessibleDetailsByAssignmentIdAsync(
            Guid assignmentId,
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            var details = await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join program in _context.TrainingPrograms.AsNoTracking()
                        on assignment.TrainingProgramId equals program.Id
                    join trainer in _context.DomainUsers.AsNoTracking()
                        on assignment.TrainerId equals trainer.Id
                    where assignment.Id == assignmentId &&
                          assignment.ClientId == clientId &&
                          assignment.Status == AssignedProgramStatus.Active &&
                          (!assignment.ExpiresAtUtc.HasValue || assignment.ExpiresAtUtc > utcNow)
                    select new
                    {
                        AssignmentId = assignment.Id,
                        ProgramId = program.Id,
                        program.Title,
                        TrainerId = trainer.Id,
                        TrainerName = trainer.FullName,
                        program.WeeksCount,
                        program.DaysPerWeek,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc
                    })
                .FirstOrDefaultAsync(cancellationToken);

            if (details is null)
            {
                return null;
            }

            var workouts = await (
                    from trainingProgramWorkout in _context.TrainingProgramWorkouts.AsNoTracking()
                    join workout in _context.Workouts.AsNoTracking()
                        on trainingProgramWorkout.WorkoutId equals workout.Id
                    where trainingProgramWorkout.TrainingProgramId == details.ProgramId
                    orderby trainingProgramWorkout.WeekNumber,
                        trainingProgramWorkout.DayNumber,
                        trainingProgramWorkout.OrderInDay,
                        trainingProgramWorkout.Id
                    select new ClientAssignedTrainingProgramWorkoutDto(
                        trainingProgramWorkout.Id,
                        workout.Id,
                        workout.Name,
                        workout.TrainerId,
                        trainingProgramWorkout.WeekNumber,
                        trainingProgramWorkout.DayNumber,
                        trainingProgramWorkout.OrderInDay,
                        Array.Empty<WorkoutExerciseDetailsDto>()))
                .ToListAsync(cancellationToken);

            if (workouts.Count > 0)
            {
                var workoutIds = workouts
                    .Select(workout => workout.WorkoutId)
                    .ToArray();

                var exercises = await (
                        from workoutExercise in _context.WorkoutExercises.AsNoTracking()
                        join exercise in _context.Exercises.AsNoTracking()
                            on workoutExercise.ExerciseId equals exercise.Id
                        join mediaAsset in _context.MediaAssets.AsNoTracking()
                            on exercise.MediaAssetId equals mediaAsset.Id into mediaAssets
                        from mediaAsset in mediaAssets.DefaultIfEmpty()
                        where workoutIds.Contains(workoutExercise.WorkoutId)
                        orderby workoutExercise.Order, workoutExercise.Id
                        select new
                        {
                            workoutExercise.WorkoutId,
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
                    .ToListAsync(cancellationToken);

                var exercisesByWorkout = exercises
                    .GroupBy(exercise => exercise.WorkoutId)
                    .ToDictionary(
                        group => group.Key,
                        group => (IReadOnlyList<WorkoutExerciseDetailsDto>)group
                            .Select(exercise => new WorkoutExerciseDetailsDto(
                                exercise.WorkoutExerciseId,
                                exercise.ExerciseId,
                                exercise.Order,
                                exercise.ExerciseName,
                                exercise.ExerciseDescription,
                                MediaAssetProjectionMapper.ToPreviewDto(exercise.ExerciseMediaAsset),
                                exercise.ExerciseMuscleGroup,
                                exercise.ExerciseEquipment,
                                exercise.Repetitions,
                                exercise.Sets,
                                exercise.LoadKg,
                                exercise.RestSeconds,
                                exercise.TrainerNote))
                            .ToList());

                workouts = workouts
                    .Select(workout => workout with
                    {
                        Exercises = exercisesByWorkout.TryGetValue(workout.WorkoutId, out var workoutExercises)
                            ? workoutExercises
                            : Array.Empty<WorkoutExerciseDetailsDto>()
                    })
                    .ToList();
            }

            return new ClientAssignedTrainingProgramDetailsDto(
                details.AssignmentId,
                details.ProgramId,
                details.Title,
                details.TrainerId,
                details.TrainerName,
                details.WeeksCount,
                details.DaysPerWeek,
                details.AssignedAtUtc,
                details.ExpiresAtUtc,
                workouts);
        }
    }
}
