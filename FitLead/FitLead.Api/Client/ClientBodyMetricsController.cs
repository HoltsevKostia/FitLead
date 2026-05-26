using FitLead.Api.Client.Contracts;
using FitLead.Api.Common.Results;
using FitLead.Application.Clients.BodyMetrics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Client
{
    [ApiController]
    [Route("api/client/body-metrics")]
    [Authorize(Policy = "ClientOnly")]
    public sealed class ClientBodyMetricsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientBodyMetricsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetClientBodyMetricEntriesQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] BodyMetricEntryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateClientBodyMetricEntryCommand(
                    request.RecordedAt,
                    request.WeightKg,
                    request.BodyFatPercent,
                    request.ChestCm,
                    request.WaistCm,
                    request.HipsCm,
                    request.ArmCm,
                    request.ThighCm,
                    request.Note),
                cancellationToken);

            return result.ToCreated(this);
        }

        [HttpPut("{entryId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Guid entryId,
            [FromBody] BodyMetricEntryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateClientBodyMetricEntryCommand(
                    entryId,
                    request.RecordedAt,
                    request.WeightKg,
                    request.BodyFatPercent,
                    request.ChestCm,
                    request.WaistCm,
                    request.HipsCm,
                    request.ArmCm,
                    request.ThighCm,
                    request.Note),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpDelete("{entryId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            Guid entryId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteClientBodyMetricEntryCommand(entryId),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
