using FitLead.Common.Results;

namespace FitLead.Application.Users.Access
{
    public interface ITrainerClientAccessLoader
    {
        Task<Result<TrainerClientAccessContext>> GetAccessibleClientAsync(
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
