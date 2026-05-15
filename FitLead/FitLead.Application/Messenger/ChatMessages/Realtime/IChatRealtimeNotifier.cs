using FitLead.Application.Messenger.ChatMessages.Queries;

namespace FitLead.Application.Messenger.ChatMessages.Realtime
{
    public interface IChatRealtimeNotifier
    {
        Task MessageCreatedAsync(
            ChatMessageDto message,
            CancellationToken cancellationToken);
    }
}
