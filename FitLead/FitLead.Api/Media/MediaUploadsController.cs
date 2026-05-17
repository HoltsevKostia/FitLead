using FitLead.Application.Media.Uploadcare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Media
{
    [ApiController]
    [Route("api/media")]
    public sealed class MediaUploadsController : ControllerBase
    {
        private readonly IUploadcareUploadSignatureService _uploadSignatureService;

        public MediaUploadsController(IUploadcareUploadSignatureService uploadSignatureService)
        {
            _uploadSignatureService = uploadSignatureService;
        }

        [Authorize]
        [HttpGet("upload-signature")]
        public IActionResult GetUploadSignature()
        {
            return Ok(_uploadSignatureService.Create());
        }
    }
}
