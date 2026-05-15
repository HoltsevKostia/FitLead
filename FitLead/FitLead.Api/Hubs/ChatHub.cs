using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FitLead.Api.Hubs
{
    [Authorize]
    public sealed class ChatHub : Hub
    {
        public Task<string> Ping()
        {
            return Task.FromResult("pong");
        }
    }
}
