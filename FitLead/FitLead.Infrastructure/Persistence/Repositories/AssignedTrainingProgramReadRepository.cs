using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
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
                select new TrainingProgramWorkoutDto(
                    tpw.Id,
                    workout.Id,
                    workout.Name,
                    workout.TrainerId,
                    tpw.WeekNumber,
                    tpw.DayNumber,
                    tpw.OrderInDay))
                .ToListAsync(cancellationToken);

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
