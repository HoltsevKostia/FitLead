using FitLead.Api.Common.Results;
using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Client
{
    [ApiController]
    [Route("api/client/training-programs")]
    [Authorize(Policy = "ClientOnly")]
    public sealed class ClientTrainingProgramsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientTrainingProgramsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignedPrograms(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetClientAssignedTrainingProgramsQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [HttpGet("{assignmentId:guid}")]
        public async Task<IActionResult> GetAssignedProgramDetails(
            Guid assignmentId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetClientAssignedTrainingProgramDetailsQuery(assignmentId),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
