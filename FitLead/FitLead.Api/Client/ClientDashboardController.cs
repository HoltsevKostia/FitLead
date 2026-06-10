using FitLead.Api.Common.Results;
using FitLead.Application.ClientDashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Client
{
    [ApiController]
    [Authorize(Policy = "ClientOnly")]
    [Route("api/client/dashboard")]
    public sealed class ClientDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetClientDashboardQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
