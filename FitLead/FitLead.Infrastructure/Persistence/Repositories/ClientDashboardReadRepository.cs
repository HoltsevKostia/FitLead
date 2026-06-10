using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.ClientDashboard.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.WorkoutLogs;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ClientDashboardReadRepository
        : IClientDashboardReadRepository
    {
        private readonly FitLeadDbContext _context;

        public ClientDashboardReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<ClientDashboardDto> GetAsync(
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            var trainer = await (
                    from relationship in _context.TrainerClients.AsNoTracking()
                    join user in _context.DomainUsers.AsNoTracking()
                        on relationship.TrainerId equals user.Id
                    where relationship.ClientId == clientId
                    orderby relationship.CreatedAt descending, relationship.TrainerId
                    select new ClientDashboardTrainerDto(
                        relationship.TrainerId,
                        user.FullName))
                .FirstOrDefaultAsync(cancellationToken);

            var assignments = await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join program in _context.TrainingPrograms.AsNoTracking()
                        on assignment.TrainingProgramId equals program.Id
                    where assignment.ClientId == clientId &&
                          assignment.Status == AssignedProgramStatus.Active &&
                          (!assignment.ExpiresAtUtc.HasValue ||
                           assignment.ExpiresAtUtc > utcNow)
                    orderby assignment.AssignedAtUtc descending, assignment.Id
                    select new
                    {
                        AssignmentId = assignment.Id,
                        ProgramId = program.Id,
                        program.Title,
                        program.WeeksCount,
                        program.DaysPerWeek,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc
                    })
                .ToListAsync(cancellationToken);

            if (assignments.Count == 0)
            {
                return new ClientDashboardDto(
                    trainer,
                    Array.Empty<ClientDashboardProgramDto>());
            }

            var assignmentIds = assignments
                .Select(assignment => assignment.AssignmentId)
                .ToArray();

            var workouts = await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join programWorkout in _context.TrainingProgramWorkouts.AsNoTracking()
                        on assignment.TrainingProgramId equals programWorkout.TrainingProgramId
                    join workout in _context.Workouts.AsNoTracking()
                        on programWorkout.WorkoutId equals workout.Id
                    join workoutLog in _context.WorkoutLogs.AsNoTracking()
                        on new
                        {
                            AssignmentId = assignment.Id,
                            ProgramWorkoutId = programWorkout.Id
                        }
                        equals new
                        {
                            AssignmentId = workoutLog.AssignedTrainingProgramId,
                            ProgramWorkoutId = workoutLog.TrainingProgramWorkoutId
                        }
                        into workoutLogs
                    from workoutLog in workoutLogs.DefaultIfEmpty()
                    where assignmentIds.Contains(assignment.Id)
                    orderby programWorkout.WeekNumber,
                        programWorkout.DayNumber,
                        programWorkout.OrderInDay,
                        programWorkout.Id
                    select new
                    {
                        AssignmentId = assignment.Id,
                        ProgramWorkoutId = programWorkout.Id,
                        WorkoutId = workout.Id,
                        WorkoutName = workout.Name,
                        programWorkout.WeekNumber,
                        programWorkout.DayNumber,
                        programWorkout.OrderInDay,
                        LogStatus = workoutLog == null
                            ? (WorkoutLogStatus?)null
                            : workoutLog.Status
                    })
                .ToListAsync(cancellationToken);

            var workoutsByAssignment = workouts
                .GroupBy(workout => workout.AssignmentId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var programs = assignments
                .Select(assignment =>
                {
                    var assignmentWorkouts = workoutsByAssignment.TryGetValue(
                        assignment.AssignmentId,
                        out var groupedWorkouts)
                        ? groupedWorkouts
                        : [];

                    var completedCount = assignmentWorkouts.Count(
                        workout => workout.LogStatus == WorkoutLogStatus.Completed);
                    var skippedCount = assignmentWorkouts.Count(
                        workout => workout.LogStatus == WorkoutLogStatus.Skipped);
                    var pendingWorkouts = assignmentWorkouts
                        .Where(workout => !workout.LogStatus.HasValue)
                        .ToList();
                    var nextWorkout = pendingWorkouts
                        .OrderBy(workout => workout.WeekNumber)
                        .ThenBy(workout => workout.DayNumber)
                        .ThenBy(workout => workout.OrderInDay)
                        .ThenBy(workout => workout.ProgramWorkoutId)
                        .FirstOrDefault();

                    return new ClientDashboardProgramDto(
                        assignment.AssignmentId,
                        assignment.ProgramId,
                        assignment.Title,
                        assignment.WeeksCount,
                        assignment.DaysPerWeek,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc,
                        completedCount,
                        skippedCount,
                        pendingWorkouts.Count,
                        nextWorkout is null
                            ? null
                            : new ClientDashboardNextWorkoutDto(
                                nextWorkout.ProgramWorkoutId,
                                nextWorkout.WorkoutId,
                                nextWorkout.WorkoutName,
                                nextWorkout.WeekNumber,
                                nextWorkout.DayNumber,
                                nextWorkout.OrderInDay));
                })
                .ToList();

            return new ClientDashboardDto(trainer, programs);
        }
    }
}
