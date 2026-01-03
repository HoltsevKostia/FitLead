using FitLead.Api.Contracts.Trainings;
using FitLead.Application.Trainings.Commands.Workouts;
using FitLead.Application.Trainings.Queries.Workout;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Controllers
{
    [ApiController]
    [Route("api/workouts")]
    public sealed class WorkoutController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkoutController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateWorkoutCommand(
                    request.TrainerId,
                    request.Name),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{workoutId:guid}/exercises")]
        public async Task<IActionResult> AddExercise(
            Guid workoutId,
            [FromBody] AddExerciseToWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AddExerciseToWorkoutCommand(
                    workoutId,
                    request.ExerciseId,
                    request.Repetitions,
                    request.Sets,
                    request.RestSeconds),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpDelete("{workoutId:guid}/exercises/{workoutExerciseId:guid}")]
        public async Task<IActionResult> RemoveExercise(
            Guid workoutId,
            Guid workoutExerciseId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RemoveExerciseFromWorkoutCommand(
                    workoutId,
                    workoutExerciseId),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpGet("trainer/{trainerId:guid}")]
        public async Task<IActionResult> GetByTrainer(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutsByTrainerQuery(trainerId),
                cancellationToken);

            return Ok(result);
        }
    }
}
