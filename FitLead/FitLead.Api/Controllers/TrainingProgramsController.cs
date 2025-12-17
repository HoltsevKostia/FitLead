using FitLead.Api.Contracts.Trainings;
using FitLead.Application.Trainings.CreateTrainingProgram;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Controllers
{
    [ApiController]
    [Route("api/training-programs")]
    public class TrainingProgramsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrainingProgramsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateTrainingProgramRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateTrainingProgramCommand(
                request.TrainerId,
                request.Title
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return CreatedAtAction(
                nameof(Create),
                new { id = result.Value },
                result.Value
            );
        }
    }
}
