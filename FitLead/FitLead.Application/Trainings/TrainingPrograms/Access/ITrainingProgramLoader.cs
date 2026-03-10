using FitLead.Common.Results;
using FitLead.Domain.Trainings;

namespace FitLead.Application.Trainings.TrainingPrograms.Access
{
    public interface ITrainingProgramLoader
    {
        Task<Result<TrainingProgram>> GetOwnedOrNotFoundAsync(
            Guid programId,
            CancellationToken cancellationToken);
    }
}
