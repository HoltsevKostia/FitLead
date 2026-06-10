using FitLead.Api.Common.Results;
using FitLead.Application.TrainerVideoReports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Trainer
{
    [ApiController]
    [Authorize(Policy = "TrainerOnly")]
    [Route("api/trainer/video-reports")]
    public sealed class TrainerVideoReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrainerVideoReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainerPendingVideoReportsQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
