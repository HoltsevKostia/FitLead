using FitLead.Api.Contracts.Trainings;
using FitLead.Application.Trainings.CreateTrainingProgram;
using FitLead.Application.Trainings.Queries;
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
    }
}
