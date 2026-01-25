using FitLead.Api.Contracts.Trainings;
using FitLead.Application.Trainings.TrainingPrograms.Commands;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.Application.Trainings.Workouts.Queries;
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
        CreateTrainingProgramCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return CreatedAtAction(
                nameof(GetByTrainer),
                new { trainerId = command.TrainerId },
                result.Value);
        }

        [HttpGet("trainer/{trainerId:guid}")]
        public async Task<IActionResult> GetByTrainer(Guid trainerId)
        {
            var programs = await _mediator.Send(
                new GetTrainingProgramsByTrainerIdQuery(trainerId));

            return Ok(programs);
        }

        [HttpGet("{programId:guid}/workouts")]
        public async Task<ActionResult<IReadOnlyList<WorkoutDto>>> GetWorkouts(
        Guid programId,
        [FromQuery] Guid trainerId,
        CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetWorkoutsByProgramIdQuery(
                    programId,
                    trainerId),
                cancellationToken);

            return Ok(result);
        }

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

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

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

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPut("{programId:guid}/workouts/order")]
        public async Task<IActionResult> ReorderWorkouts(
            Guid programId,
            [FromBody] ReorderProgramWorkoutsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ReorderProgramWorkoutsCommand(programId, request.WorkoutIds),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}
