using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.WorkoutLogs;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientProgramsReadRepository : ITrainerClientProgramsReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientProgramsReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainerClientProgramDto>> GetProgramsAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var assignments = await (
                    from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                    join program in _context.TrainingPrograms.AsNoTracking()
                        on assignment.TrainingProgramId equals program.Id
                    where assignment.TrainerId == trainerId &&
                          assignment.ClientId == clientId
                    orderby assignment.Status == AssignedProgramStatus.Active ? 0 : 1,
                        assignment.AssignedAtUtc descending,
                        assignment.Id descending
                    select new
                    {
                        AssignmentId = assignment.Id,
                        ProgramId = program.Id,
                        ProgramTitle = program.Title,
                        assignment.Status,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc,
                        assignment.RevokedAtUtc
                    })
                .ToListAsync(cancellationToken);

            if (assignments.Count == 0)
            {
                return Array.Empty<TrainerClientProgramDto>();
            }

            var programIds = assignments
                .Select(assignment => assignment.ProgramId)
                .Distinct()
                .ToArray();
            var assignmentIds = assignments
                .Select(assignment => assignment.AssignmentId)
                .ToArray();

            var totalWorkoutsByProgramId = await _context.TrainingProgramWorkouts
                .AsNoTracking()
                .Where(workout => programIds.Contains(workout.TrainingProgramId))
                .GroupBy(workout => workout.TrainingProgramId)
                .Select(group => new
                {
                    ProgramId = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(
                    item => item.ProgramId,
                    item => item.Count,
                    cancellationToken);

            var logCounts = await _context.WorkoutLogs
                .AsNoTracking()
                .Where(log => assignmentIds.Contains(log.AssignedTrainingProgramId))
                .GroupBy(log => new { log.AssignedTrainingProgramId, log.Status })
                .Select(group => new
                {
                    group.Key.AssignedTrainingProgramId,
                    group.Key.Status,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);

            var logCountsByAssignmentId = logCounts
                .GroupBy(log => log.AssignedTrainingProgramId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());

            return assignments
                .Select(assignment =>
                {
                    totalWorkoutsByProgramId.TryGetValue(
                        assignment.ProgramId,
                        out var totalWorkouts);
                    logCountsByAssignmentId.TryGetValue(
                        assignment.AssignmentId,
                        out var assignmentLogCounts);

                    var completed = assignmentLogCounts?
                        .Where(log => log.Status == WorkoutLogStatus.Completed)
                        .Sum(log => log.Count) ?? 0;
                    var skipped = assignmentLogCounts?
                        .Where(log => log.Status == WorkoutLogStatus.Skipped)
                        .Sum(log => log.Count) ?? 0;
                    var pending = Math.Max(0, totalWorkouts - completed - skipped);

                    return new TrainerClientProgramDto(
                        assignment.AssignmentId,
                        assignment.ProgramId,
                        assignment.ProgramTitle,
                        assignment.Status.ToString(),
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc,
                        assignment.RevokedAtUtc,
                        totalWorkouts,
                        new TrainerClientWorkoutLogCountsDto(completed, skipped, pending));
                })
                .ToList();
        }
    }
}
