using FitLead.Api.Common.Results;
using Microsoft.AspNetCore.Authorization;
using FitLead.Application.Trainings.Exercises.Commands;
using FitLead.Application.Trainings.Exercises.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FitLead.Api.Exercises.Contracts;

namespace FitLead.Api.Exercises
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

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateExerciseCommand(
                    request.Name,
                    request.Description,
                    request.MediaAssetId,
                    request.MuscleGroup,
                    request.Equipment),
                cancellationToken);

            return result.ToCreated(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpPost("{exerciseId:guid}/copy-to-my-library")]
        public async Task<IActionResult> CopyToMyLibrary(
            Guid exerciseId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CopyExerciseToMyLibraryCommand(exerciseId),
                cancellationToken);

            return result.ToCreated(this);
        }
       
        [Authorize(Policy = "TrainerOnly")]
        [HttpGet]
        public async Task<IActionResult> GetByTrainer(
            CancellationToken cancellationToken,
            [FromQuery] ExerciseListSource source = ExerciseListSource.All)
        {
            var exercises = await _mediator.Send(
                new GetExercisesByTrainerQuery(source),
                cancellationToken);

            return exercises.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpGet("{exerciseId:guid}")]
        public async Task<IActionResult> GetById(
            Guid exerciseId,
            CancellationToken cancellationToken)
        {
            var exercise = await _mediator.Send(
                new GetExerciseByIdQuery(exerciseId),
                cancellationToken);

            return exercise.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
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
                    request.MediaAssetId,
                    request.MuscleGroup,
                    request.Equipment),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
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

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpPost("{exerciseId:guid}/deletion-confirmations")]
        public async Task<IActionResult> ConfirmDelete(
            Guid exerciseId,
            [FromBody] ConfirmDeleteExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ConfirmDeleteExerciseCommand(exerciseId, request.Token),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
