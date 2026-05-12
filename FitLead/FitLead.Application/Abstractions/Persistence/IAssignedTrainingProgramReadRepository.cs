using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IAssignedTrainingProgramReadRepository
    {
        Task<IReadOnlyList<TrainingProgramAssignmentDto>> GetByProgramIdAsync(
            Guid trainingProgramId,
            CancellationToken cancellationToken);
    }
}
