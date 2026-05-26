using FitLead.Api.Client.Contracts;
using FitLead.Api.Common.Results;
using FitLead.Application.Clients.ClientProfiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Client
{
    [ApiController]
    [Route("api/client/profile")]
    [Authorize(Policy = "ClientOnly")]
    public sealed class ClientProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetClientProfileQuery(), cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            [FromBody] UpdateClientProfileRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateClientProfileCommand(
                    request.Goal,
                    request.ExperienceLevel,
                    request.HeightCm,
                    request.Limitations,
                    request.TrainingPreferences,
                    request.AdditionalInfo),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
