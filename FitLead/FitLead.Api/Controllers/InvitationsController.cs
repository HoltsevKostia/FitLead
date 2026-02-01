using FitLead.Api.Contracts.Invitations;
using FitLead.Api.Identity;
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

        [RequireUser]
        [HttpGet("client")]
        public async Task<IActionResult> GetPendingForClient(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetPendingInvitationsForClientQuery(),
                cancellationToken);

            return Ok(result);
        }

        [RequireUser]
        [HttpGet("trainer")]
        public async Task<IActionResult> GetSentByTrainer(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetSentInvitationsByTrainerQuery(),
                cancellationToken);

            return Ok(result);
        }

        [RequireUser]
        [HttpPost]
        public async Task<IActionResult> Create(
           [FromBody] CreateInvitationRequest request,
           CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateInvitationCommand(
                    request.ClientId),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [RequireUser]
        [HttpPost("{invitationId:guid}/accept")]
        public async Task<IActionResult> Accept(
            Guid invitationId,
            [FromBody] AcceptInvitationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AcceptInvitationCommand(
                    invitationId),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        [RequireUser]
        [HttpPost("{invitationId:guid}/decline")]
        public async Task<IActionResult> Decline(
            Guid invitationId,
            [FromBody] DeclineInvitationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeclineInvitationCommand(
                    invitationId),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}
