using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientWorkoutLogsReadRepository
        : ITrainerClientWorkoutLogsReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientWorkoutLogsReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainerClientWorkoutLogDto>> GetRecentWorkoutLogsAsync(
            Guid trainerId,
            Guid clientId,
            int limit,
            CancellationToken cancellationToken)
        {
            if (limit <= 0)
            {
                return Array.Empty<TrainerClientWorkoutLogDto>();
            }

            var logs = await (
                    from workoutLog in _context.WorkoutLogs.AsNoTracking()
                    join assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                        on workoutLog.AssignedTrainingProgramId equals assignment.Id
                    join program in _context.TrainingPrograms.AsNoTracking()
                        on assignment.TrainingProgramId equals program.Id
                    join programWorkout in _context.TrainingProgramWorkouts.AsNoTracking()
                        on workoutLog.TrainingProgramWorkoutId equals programWorkout.Id
                    join workout in _context.Workouts.AsNoTracking()
                        on programWorkout.WorkoutId equals workout.Id
                    where workoutLog.TrainerId == trainerId &&
                          workoutLog.ClientId == clientId
                    orderby (workoutLog.UpdatedAtUtc ?? workoutLog.CreatedAtUtc) descending,
                        workoutLog.Id descending
                    select new
                    {
                        LogId = workoutLog.Id,
                        AssignmentId = assignment.Id,
                        ProgramId = program.Id,
                        ProgramTitle = program.Title,
                        ProgramWorkoutId = programWorkout.Id,
                        WorkoutId = workout.Id,
                        WorkoutName = workout.Name,
                        programWorkout.WeekNumber,
                        programWorkout.DayNumber,
                        programWorkout.OrderInDay,
                        workoutLog.Status,
                        workoutLog.PerformedAtUtc,
                        workoutLog.DifficultyRating,
                        workoutLog.ClientNote,
                        workoutLog.CreatedAtUtc,
                        workoutLog.UpdatedAtUtc
                    })
                .Take(limit)
                .ToListAsync(cancellationToken);

            return logs
                .Select(log => new TrainerClientWorkoutLogDto(
                    log.LogId,
                    log.AssignmentId,
                    log.ProgramId,
                    log.ProgramTitle,
                    log.ProgramWorkoutId,
                    log.WorkoutId,
                    log.WorkoutName,
                    log.WeekNumber,
                    log.DayNumber,
                    log.OrderInDay,
                    log.Status.ToString(),
                    log.PerformedAtUtc,
                    log.DifficultyRating,
                    log.ClientNote,
                    log.CreatedAtUtc,
                    log.UpdatedAtUtc))
                .ToList();
        }
    }
}
