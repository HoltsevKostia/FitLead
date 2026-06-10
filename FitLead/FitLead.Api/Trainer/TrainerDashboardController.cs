using FitLead.Api.Common.Results;
using FitLead.Application.TrainerDashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Trainer
{
    [ApiController]
    [Authorize(Policy = "TrainerOnly")]
    [Route("api/trainer/dashboard")]
    public sealed class TrainerDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrainerDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainerDashboardSummaryQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
