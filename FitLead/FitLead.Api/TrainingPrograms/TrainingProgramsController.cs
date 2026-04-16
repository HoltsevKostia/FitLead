using FitLead.Api.Common.Results;
using FitLead.Api.Contracts.Trainings;
using Microsoft.AspNetCore.Authorization;
using FitLead.Application.Trainings.TrainingPrograms.Commands;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.TrainingPrograms
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

        [Authorize(Policy = "TrainerOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(
        CreateTrainingProgramCommand command)
        {
            var result = await _mediator.Send(command);

            return result.ToCreated(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpGet]
        public async Task<IActionResult> GetByTrainer()
        {
            var programs = await _mediator.Send(
                new GetTrainingProgramsByTrainerIdQuery());

            return programs.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpGet("{programId:guid}/workouts")]
        public async Task<IActionResult> GetWorkouts(
        Guid programId,
        CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutsByProgramIdQuery(
                    programId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPost("{programId:guid}/workouts")]
        public async Task<IActionResult> AddWorkout(
            Guid programId,
            [FromBody] AddWorkoutToProgramRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AddWorkoutToProgramCommand(
                    programId,
                    request.WorkoutId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpDelete("{programId:guid}/workouts/{workoutId:guid}")]
        public async Task<IActionResult> RemoveWorkout(
            Guid programId,
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RemoveWorkoutFromProgramCommand(
                    programId,
                    workoutId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPut("{programId:guid}/workouts/order")]
        public async Task<IActionResult> ReorderWorkouts(
            Guid programId,
            [FromBody] ReorderProgramWorkoutsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ReorderProgramWorkoutsCommand(programId, request.WorkoutIds),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpDelete("{programId:guid}")]
        public async Task<IActionResult> Delete(
            Guid programId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteTrainingProgramCommand(programId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPost("{programId:guid}/deletion-confirmations")]
        public async Task<IActionResult> ConfirmDelete(
            Guid programId,
            [FromBody] ConfirmDeleteTrainingProgramRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ConfirmDeleteTrainingProgramCommand(programId, request.Token),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
