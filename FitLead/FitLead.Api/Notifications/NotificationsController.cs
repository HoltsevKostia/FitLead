using FitLead.Api.Common.Results;
using FitLead.Application.Notifications.Commands;
using FitLead.Application.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Notifications
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int? limit,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetNotificationsQuery(limit),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetUnreadNotificationCountQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkRead(
            Guid notificationId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new MarkNotificationReadCommand(notificationId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new MarkAllNotificationsReadCommand(),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
