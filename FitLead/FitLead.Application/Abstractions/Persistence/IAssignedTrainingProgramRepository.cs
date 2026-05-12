using FitLead.Domain.Trainings.TrainingProgramAssignments;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IAssignedTrainingProgramRepository
    {
        Task AddAsync(
            AssignedTrainingProgram assignment,
            CancellationToken cancellationToken);

        Task<AssignedTrainingProgram?> GetActiveByClientAndProgramAsync(
            Guid clientId,
            Guid trainingProgramId,
            CancellationToken cancellationToken);

        Task<AssignedTrainingProgram?> GetByIdForProgramAndTrainerAsync(
            Guid assignmentId,
            Guid trainingProgramId,
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
