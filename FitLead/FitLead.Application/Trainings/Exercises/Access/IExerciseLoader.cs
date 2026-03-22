using FitLead.Common.Results;
using FitLead.Domain.Trainings;

namespace FitLead.Application.Trainings.Exercises.Access
{
    public interface IExerciseLoader
    {
        Task<Result<Exercise>> GetOwnedOrNotFoundAsync(
            Guid exerciseId,
            CancellationToken cancellationToken);
    }
}
