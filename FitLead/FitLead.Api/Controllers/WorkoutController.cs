using FitLead.Api.Contracts.Trainings;
using FitLead.Api.Identity;
using FitLead.Application.Trainings.Workouts.Commands;
using FitLead.Application.Trainings.Workouts.Queries;
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

        [RequireUser]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateWorkoutCommand(
                    request.Name),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [RequireUser]
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

        [RequireUser]
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

        [RequireUser]
        [HttpGet]
        public async Task<IActionResult> GetByTrainer(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutsByTrainerQuery(),
                cancellationToken);

            return Ok(result);
        }

        [RequireUser]
        [HttpGet("{workoutId:guid}")]
        public async Task<IActionResult> GetWorkoutDetails(
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutDetailsByIdQuery(workoutId), cancellationToken);

            return Ok(result);
        }

        [RequireUser]
        [HttpPut("{workoutId:guid}/exercises/{workoutExerciseId:guid}")]
        public async Task<IActionResult> UpdateExercise(
            Guid workoutId,
            Guid workoutExerciseId,
            [FromBody] UpdateWorkoutExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateWorkoutExerciseCommand(
                    workoutId,
                    workoutExerciseId,
                    request.Repetitions,
                    request.Sets,
                    request.RestSeconds),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}
