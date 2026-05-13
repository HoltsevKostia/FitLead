using FitLead.Application.Messenger.Chats.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IChatReadRepository
    {
        Task<IReadOnlyList<ChatDto>> GetChatsForTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<ChatDto>> GetChatsForClientAsync(
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
