using FitLead.Api.Common.Results;
using FitLead.Api.Push.Contracts;
using FitLead.Application.Notifications.Push;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Push
{
    [ApiController]
    [Authorize]
    [Route("api/push")]
    public sealed class PushSubscriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PushSubscriptionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("vapid-public-key")]
        public async Task<IActionResult> GetVapidPublicKey(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetVapidPublicKeyQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("subscriptions")]
        public async Task<IActionResult> RegisterSubscription(
            [FromBody] RegisterPushSubscriptionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RegisterPushSubscriptionCommand(
                    request.Endpoint,
                    request.Keys?.P256dh ?? string.Empty,
                    request.Keys?.Auth ?? string.Empty,
                    request.UserAgent),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
