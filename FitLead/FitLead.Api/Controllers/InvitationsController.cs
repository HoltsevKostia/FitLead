using FitLead.Api.Contracts.Invitations;
using FitLead.Application.Invitations.Commands;
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

        [HttpPost]
        public async Task<IActionResult> Create(
           [FromBody] CreateInvitationRequest request,
           CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateInvitationCommand(
                    request.TrainerId,
                    request.ClientId,
                    DateTime.UtcNow),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{invitationId:guid}/accept")]
        public async Task<IActionResult> Accept(
            Guid invitationId,
            [FromBody] AcceptInvitationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AcceptInvitationCommand(
                    request.ClientId,
                    invitationId,
                    DateTime.UtcNow),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPost("{invitationId:guid}/decline")]
        public async Task<IActionResult> Decline(
            Guid invitationId,
            [FromBody] DeclineInvitationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeclineInvitationCommand(
                    request.ClientId,
                    invitationId,
                    DateTime.UtcNow),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}
