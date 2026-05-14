using FitLead.Application.Messenger.Chats.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IChatReadRepository
    {
        Task<ChatDetailsDto?> GetByIdAsync(
            Guid chatId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<ChatListItemDto>> GetChatsForTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<ChatListItemDto>> GetChatsForClientAsync(
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
