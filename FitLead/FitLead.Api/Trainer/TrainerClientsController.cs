using FitLead.Api.Common.Results;
using FitLead.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Trainer
{
    [ApiController]
    [Authorize(Policy = "TrainerOnly")]
    [Route("api/trainer/clients")]
    public sealed class TrainerClientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrainerClientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainerClientsOverviewQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpGet("{clientId:guid}/workspace")]
        public async Task<IActionResult> GetWorkspace(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainerClientWorkspaceQuery(clientId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpGet("{clientId:guid}/overview")]
        public async Task<IActionResult> GetOverviewSummary(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainerClientOverviewSummaryQuery(clientId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpGet("{clientId:guid}/programs")]
        public async Task<IActionResult> GetPrograms(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainerClientProgramsQuery(clientId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpGet("{clientId:guid}/workout-logs")]
        public async Task<IActionResult> GetWorkoutLogs(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainerClientWorkoutLogsQuery(clientId),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
