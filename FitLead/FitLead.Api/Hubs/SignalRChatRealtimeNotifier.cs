using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Application.Messenger.ChatMessages.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace FitLead.Api.Hubs
{
    public sealed class SignalRChatRealtimeNotifier : IChatRealtimeNotifier
    {
        private const string MessageCreatedEventName = "MessageCreated";

        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRChatRealtimeNotifier(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task MessageCreatedAsync(
            ChatMessageDto message,
            CancellationToken cancellationToken)
        {
            return _hubContext.Clients
                .Group(ChatHubGroups.ForChat(message.ChatId))
                .SendAsync(
                    MessageCreatedEventName,
                    message,
                    cancellationToken);
        }
    }
}
