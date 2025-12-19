using FitLead.Application.Trainings.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainingProgramReadRepository
    {
        Task<IReadOnlyList<TrainingProgramDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
