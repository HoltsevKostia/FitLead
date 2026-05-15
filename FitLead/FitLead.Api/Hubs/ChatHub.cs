using System.Security.Claims;
using FitLead.Application.Identity;
using FitLead.Application.Messenger.Chats.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FitLead.Api.Hubs
{
    [Authorize]
    public sealed class ChatHub : Hub
    {
        private readonly IChatLoader _chatLoader;

        public ChatHub(IChatLoader chatLoader)
        {
            _chatLoader = chatLoader;
        }

        public Task<string> Ping()
        {
            return Task.FromResult("pong");
        }

        public async Task JoinChat(Guid chatId)
        {
            var userId = GetRequiredDomainUserId();
            var chatResult = await _chatLoader.GetAccessibleForUserOrNotFoundAsync(
                userId,
                chatId,
                Context.ConnectionAborted);

            if (chatResult.IsFailure)
            {
                throw new HubException("Chat not found");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                ChatHubGroups.ForChat(chatId),
                Context.ConnectionAborted);
        }

        private Guid GetRequiredDomainUserId()
        {
            var claim = Context.User?.FindFirstValue(CustomClaimTypes.DomainUserId);
            if (!Guid.TryParse(claim, out var userId))
            {
                throw new HubException("Current user is missing");
            }

            return userId;
        }
    }
}
