using FitLead.Application.Messenger.ChatMessages.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IChatMessageReadRepository
    {
        Task<IReadOnlyList<ChatMessageHistoryItemDto>> GetMessagesAsync(
            Guid chatId,
            int limit,
            DateTime? beforeCreatedAtUtc,
            CancellationToken cancellationToken);
    }
}
