using FitLead.Common.Results;
using FitLead.Domain.Trainings.TrainingPrograms;

namespace FitLead.Application.Trainings.TrainingPrograms.Access
{
    public interface ITrainingProgramLoader
    {
        Task<Result<TrainingProgram>> GetOwnedOrNotFoundAsync(
            Guid programId,
            CancellationToken cancellationToken);
    }
}
