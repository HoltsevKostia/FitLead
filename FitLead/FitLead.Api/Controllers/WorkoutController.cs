using FitLead.Api.Common.Results;
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

            return result.ToCreated(this);
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

            return result.ToCreated(this);
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

            return result.ToActionResult(this);
        }

        [RequireUser]
        [HttpGet]
        public async Task<IActionResult> GetByTrainer(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutsByTrainerQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [RequireUser]
        [HttpGet("{workoutId:guid}")]
        public async Task<IActionResult> GetWorkoutDetails(
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutDetailsByIdQuery(workoutId), cancellationToken);

            return result.ToActionResult(this);
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

            return result.ToActionResult(this);
        }

        [RequireUser]
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
    }
}
