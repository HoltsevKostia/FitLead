using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FitLead.Api.Hubs
{
    [Authorize]
    public sealed class NotificationHub : Hub
    {
        public Task<string> Ping()
        {
            return Task.FromResult("pong");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetRequiredDomainUserId();
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                NotificationHubGroups.ForUser(userId),
                Context.ConnectionAborted);

            await base.OnConnectedAsync();
        }
    }
}
