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

        public async Task<IReadOnlyList<ClientAssignedTrainingProgramDto>> GetAccessibleByClientIdAsync(
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            return await (
                from assignment in _context.AssignedTrainingPrograms
                join program in _context.TrainingPrograms
                    on assignment.TrainingProgramId equals program.Id
                join trainer in _context.DomainUsers
                    on assignment.TrainerId equals trainer.Id
                where assignment.ClientId == clientId &&
                      assignment.Status == AssignedProgramStatus.Active &&
                      (!assignment.ExpiresAtUtc.HasValue || assignment.ExpiresAtUtc > utcNow)
                orderby assignment.AssignedAtUtc descending
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
                from assignment in _context.AssignedTrainingPrograms
                join program in _context.TrainingPrograms
                    on assignment.TrainingProgramId equals program.Id
                join trainer in _context.DomainUsers
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
                from tpw in _context.TrainingProgramWorkouts
                join workout in _context.Workouts
                    on tpw.WorkoutId equals workout.Id
                where tpw.TrainingProgramId == details.ProgramId
                orderby tpw.WeekNumber, tpw.DayNumber, tpw.OrderInDay
                select new ClientAssignedTrainingProgramWorkoutDto(
                    tpw.Id,
                    workout.Id,
                    workout.Name,
                    workout.TrainerId,
                    tpw.WeekNumber,
                    tpw.DayNumber,
                    tpw.OrderInDay,
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
                    where workoutIds.Contains(workoutExercise.WorkoutId)
                    orderby workoutExercise.Order
                    select new
                    {
                        workoutExercise.WorkoutId,
                        Exercise = new WorkoutExerciseDetailsDto(
                            workoutExercise.Id,
                            workoutExercise.ExerciseId,
                            workoutExercise.Order,
                            exercise.Name,
                            exercise.Description,
                            exercise.MediaUrl != null ? exercise.MediaUrl.Value : null,
                            exercise.MuscleGroup,
                            exercise.Equipment,
                            workoutExercise.Repetitions,
                            workoutExercise.Sets,
                            workoutExercise.LoadKg,
                            workoutExercise.RestSeconds,
                            workoutExercise.TrainerNote)
                    })
                    .ToListAsync(cancellationToken);

                var exercisesByWorkout = exercises
                    .GroupBy(exercise => exercise.WorkoutId)
                    .ToDictionary(
                        group => group.Key,
                        group => (IReadOnlyList<WorkoutExerciseDetailsDto>)group
                            .Select(exercise => exercise.Exercise)
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
