using FitLead.Api.Common.Results;
using Microsoft.AspNetCore.Authorization;
using FitLead.Application.Users.Commands;
using FitLead.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Users
{
    [ApiController]
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserCommand command)
        {
            var result = await _mediator.Send(command);

            return result.ToCreated(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpGet("clients")]
        public async Task<IActionResult> GetClientsByTrainer()
        {
            var clients = await _mediator.Send(
                new GetClientsByTrainerIdQuery());

            return clients.ToActionResult(this);
        }

        [Authorize(Policy = "ClientOnly")]
        [HttpGet("my-trainer")]
        public async Task<IActionResult> GetMyTrainer(CancellationToken cancellationToken)
        {
            var trainer = await _mediator.Send(
                new GetMyTrainerQuery(),
                cancellationToken);

            return trainer.ToActionResult(this);
        }
    }
}



