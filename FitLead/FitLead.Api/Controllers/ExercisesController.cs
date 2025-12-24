using FitLead.Api.Contracts.Trainings;
using FitLead.Application.Trainings.Commands.CreateExercise;
using FitLead.Application.Trainings.Queries.Exercise;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Controllers
{
    [ApiController]
    [Route("api/exercises")]
    public sealed class ExercisesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExercisesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateExerciseCommand(
                    request.TrainerId,
                    request.Name,
                    request.Description,
                    request.MediaUrl),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("trainer/{trainerId:guid}")]
        public async Task<IActionResult> GetByTrainer(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var exercises = await _mediator.Send(
                new GetExercisesByTrainerQuery(trainerId),
                cancellationToken);

            return Ok(exercises);
        }
    }
}
