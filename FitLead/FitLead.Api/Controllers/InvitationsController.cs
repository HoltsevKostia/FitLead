using FitLead.Application.Invitations.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Controllers
{
    [ApiController]
    [Route("api/invitations")]
    public sealed class InvitationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvitationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetPendingForClient(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetPendingInvitationsForClientQuery(
                    clientId,
                    DateTime.UtcNow),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("trainer/{trainerId}")]
        public async Task<IActionResult> GetSentByTrainer(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetSentInvitationsByTrainerQuery(trainerId),
                cancellationToken);

            return Ok(result);
        }
    }
}
