using FitLead.Api.Client.Contracts;
using FitLead.Api.Common.Results;
using FitLead.Application.Clients.ProgressPhotos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Client
{
    [ApiController]
    [Route("api/client/progress-photos")]
    [Authorize(Policy = "ClientOnly")]
    public sealed class ClientProgressPhotosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientProgressPhotosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetClientProgressPhotosQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] ProgressPhotoRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateClientProgressPhotoCommand(
                    request.MediaAssetId,
                    request.TakenAt,
                    request.Label,
                    request.Note),
                cancellationToken);

            return result.ToCreated(this);
        }

        [HttpDelete("{photoId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            Guid photoId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteClientProgressPhotoCommand(photoId),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
