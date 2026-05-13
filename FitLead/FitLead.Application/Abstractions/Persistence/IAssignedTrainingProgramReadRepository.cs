using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IAssignedTrainingProgramReadRepository
    {
        Task<IReadOnlyList<TrainingProgramAssignmentDto>> GetByProgramIdAsync(
            Guid trainingProgramId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<ClientAssignedTrainingProgramDto>> GetAccessibleByClientIdAsync(
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken);

        Task<ClientAssignedTrainingProgramDetailsDto?> GetAccessibleDetailsByAssignmentIdAsync(
            Guid assignmentId,
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken);
    }
}
