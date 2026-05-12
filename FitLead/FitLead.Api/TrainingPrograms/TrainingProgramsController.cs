using FitLead.Api.Common.Results;
using FitLead.Api.TrainingPrograms.Contracts;
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
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTrainingProgramRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateTrainingProgramCommand(
                    request.Title,
                    request.WeeksCount,
                    request.DaysPerWeek),
                cancellationToken);

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
        [HttpGet("{programId:guid}")]
        public async Task<IActionResult> GetById(
            Guid programId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTrainingProgramByIdQuery(programId),
                cancellationToken);

            return result.ToActionResult(this);
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
        [ValidateAntiForgeryToken]
        [HttpPost("{programId:guid}/workouts")]
        public async Task<IActionResult> AddWorkout(
            Guid programId,
            [FromBody] AddWorkoutToProgramRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AddWorkoutToProgramCommand(
                    programId,
                    request.WorkoutId,
                    request.WeekNumber,
                    request.DayNumber),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpDelete("{programId:guid}/workouts/{trainingProgramWorkoutId:guid}")]
        public async Task<IActionResult> RemoveWorkout(
            Guid programId,
            Guid trainingProgramWorkoutId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RemoveWorkoutFromProgramCommand(
                    programId,
                    trainingProgramWorkoutId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpPut("{programId:guid}/workouts/order")]
        public async Task<IActionResult> ReorderWorkouts(
            Guid programId,
            [FromBody] ReorderProgramWorkoutsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ReorderProgramWorkoutsCommand(
                    programId,
                    request.WeekNumber,
                    request.DayNumber,
                    request.EntryIds),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpPut("{programId:guid}/workouts/{trainingProgramWorkoutId:guid}/position")]
        public async Task<IActionResult> MoveWorkout(
            Guid programId,
            Guid trainingProgramWorkoutId,
            [FromBody] MoveWorkoutEntryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new MoveWorkoutEntryCommand(
                    programId,
                    trainingProgramWorkoutId,
                    request.TargetWeekNumber,
                    request.TargetDayNumber,
                    request.TargetOrderInDay),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
