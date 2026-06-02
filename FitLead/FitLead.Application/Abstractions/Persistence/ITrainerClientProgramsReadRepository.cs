using FitLead.Application.Users.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientProgramsReadRepository
    {
        Task<IReadOnlyList<TrainerClientProgramDto>> GetProgramsAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
