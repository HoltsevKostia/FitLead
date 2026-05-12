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
    }
}
