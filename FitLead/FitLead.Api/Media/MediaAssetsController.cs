using FitLead.Api.Common.Results;
using FitLead.Api.Media.Contracts;
using FitLead.Application.Media.MediaAssets.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Media
{
    [ApiController]
    [Route("api/media/assets")]
    public sealed class MediaAssetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MediaAssetsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterMediaAssetRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RegisterMediaAssetCommand(
                    request.StorageProvider,
                    request.StorageObjectId,
                    request.DeliveryUrl,
                    request.FileName,
                    request.ContentType,
                    request.SizeBytes,
                    request.Kind,
                    request.DurationSeconds),
                cancellationToken);

            return result.ToCreated(this);
        }
    }
}
