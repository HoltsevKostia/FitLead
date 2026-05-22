using System.Security.Claims;
using FitLead.Application.Identity;
using Microsoft.AspNetCore.SignalR;

namespace FitLead.Api.Hubs
{
    public static class HubCallerContextExtensions
    {
        public static Guid GetRequiredDomainUserId(this HubCallerContext context)
        {
            var claim = context.User?.FindFirstValue(CustomClaimTypes.DomainUserId);
            if (!Guid.TryParse(claim, out var userId))
            {
                throw new HubException("Current user is missing");
            }

            return userId;
        }
    }
}
