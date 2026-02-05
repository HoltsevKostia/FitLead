using FitLead.Api.Common.Results;
using FitLead.Api.Identity;
using FitLead.Application.Users.Commands;
using FitLead.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Controllers
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

        [RequireUser]
        [HttpGet("clients")]
        public async Task<IActionResult> GetClientsByTrainer()
        {
            var clients = await _mediator.Send(
                new GetClientsByTrainerIdQuery());

            return clients.ToActionResult(this);
        }
    }
}



