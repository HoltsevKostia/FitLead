using FitLead.Domain.Messenger.ChatMessages;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IChatMessageRepository
    {
        Task AddAsync(
            ChatMessage message,
            CancellationToken cancellationToken);
    }
}
