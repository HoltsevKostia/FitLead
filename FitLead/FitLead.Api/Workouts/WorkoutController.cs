using FitLead.Api.Common.Results;
using Microsoft.AspNetCore.Authorization;
using FitLead.Application.Trainings.Workouts.Commands;
using FitLead.Application.Trainings.Workouts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FitLead.Api.Exercises.Contracts;
using FitLead.Api.Workouts.Contracts;

namespace FitLead.Api.Workouts
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

        [Authorize(Policy = "TrainerOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateWorkoutCommand(
                    request.Name),
                cancellationToken);

            return result.ToCreated(this);
        }

        [Authorize(Policy = "TrainerOnly")]
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

            return result.ToCreated(this);
        }

        [Authorize(Policy = "TrainerOnly")]
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

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpGet]
        public async Task<IActionResult> GetByTrainer(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutsByTrainerQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpGet("{workoutId:guid}")]
        public async Task<IActionResult> GetWorkoutDetails(
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutDetailsByIdQuery(workoutId), cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
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

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPut("{workoutId:guid}/name")]
        public async Task<IActionResult> Rename(
            Guid workoutId,
            [FromBody] RenameWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RenameWorkoutCommand(workoutId, request.Name),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpDelete("{workoutId:guid}")]
        public async Task<IActionResult> Delete(
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteWorkoutCommand(workoutId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPost("{workoutId:guid}/deletion-confirmations")]
        public async Task<IActionResult> ConfirmDelete(
            Guid workoutId,
            [FromBody] ConfirmDeleteWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ConfirmDeleteWorkoutCommand(workoutId, request.Token),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
