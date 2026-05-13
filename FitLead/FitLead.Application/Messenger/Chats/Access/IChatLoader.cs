using FitLead.Common.Results;
using FitLead.Domain.Messenger.Chats;

namespace FitLead.Application.Messenger.Chats.Access
{
    public interface IChatLoader
    {
        Task<Result<Chat>> GetAccessibleOrNotFoundAsync(
            Guid chatId,
            CancellationToken cancellationToken);

        Task<Result> EnsureCurrentTrainerHasClientAsync(
            Guid clientId,
            CancellationToken cancellationToken);

        Task<Result> EnsureCurrentClientHasTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
