using FitLead.Api.Common.Results;
using FitLead.Api.Contracts.Trainings;
using FitLead.Api.Identity;
using FitLead.Application.Trainings.Exercises.Commands;
using FitLead.Application.Trainings.Exercises.Queries;
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

        [RequireUser]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateExerciseCommand(
                    request.Name,
                    request.Description,
                    request.MediaUrl),
                cancellationToken);

            return result.ToCreated(this);
        }
       
        [RequireUser]
        [HttpGet]
        public async Task<IActionResult> GetByTrainer(
            CancellationToken cancellationToken)
        {
            var exercises = await _mediator.Send(
                new GetExercisesByTrainerQuery(),
                cancellationToken);

            return exercises.ToActionResult(this);
        }

        [RequireUser]
        [HttpPut("{exerciseId:guid}")]
        public async Task<IActionResult> Update(
            Guid exerciseId,
            UpdateExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateExerciseCommand(
                    exerciseId,
                    request.Name,
                    request.Description,
                    request.MediaUrl),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [RequireUser]
        [HttpDelete("{exerciseId:guid}")]
        public async Task<IActionResult> Delete(
            Guid exerciseId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteExerciseCommand(exerciseId),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
