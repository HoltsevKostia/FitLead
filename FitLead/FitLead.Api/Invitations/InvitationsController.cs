using FitLead.Api.Common.Results;
using FitLead.Api.Invitations.Contracts;
using FitLead.Application.Invitations.Commands;
using FitLead.Application.Invitations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Invitations
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

        [Authorize(Policy = "TrainerOnly")]
        [HttpGet("trainer")]
        public async Task<IActionResult> GetSentByTrainer(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetSentInvitationsByTrainerQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpGet("{token}/preview")]
        public async Task<IActionResult> Preview(
            string token,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetInvitationPreviewQuery(token),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateInvitationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateInvitationCommand(request.ExpiresInDays),
                cancellationToken);

            return result.ToCreated(this);
        }

        [Authorize(Policy = "ClientOnly")]
        [HttpPost("{token}/accept")]
        public async Task<IActionResult> Accept(
            string token,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AcceptInvitationCommand(token),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
