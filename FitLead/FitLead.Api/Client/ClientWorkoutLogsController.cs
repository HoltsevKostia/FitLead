using FitLead.Api.Client.Contracts;
using FitLead.Api.Common.Results;
using FitLead.Application.Trainings.WorkoutLogs.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Client
{
    [ApiController]
    [Route("api/client/training-program-assignments/{assignmentId:guid}/workouts/{programWorkoutId:guid}/log")]
    [Authorize(Policy = "ClientOnly")]
    public sealed class ClientWorkoutLogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientWorkoutLogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Guid assignmentId,
            Guid programWorkoutId,
            [FromBody] UpsertWorkoutLogRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpsertWorkoutLogCommand(
                    assignmentId,
                    programWorkoutId,
                    request.Status,
                    request.PerformedAtUtc,
                    request.ClientNote,
                    request.DifficultyRating),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
