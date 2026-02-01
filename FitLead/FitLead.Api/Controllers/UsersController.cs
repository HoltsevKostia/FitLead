using FitLead.Api.Identity;
using FitLead.Application.Users.Commands.CreateUser;
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

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { userId = result.Value });
        }

        [RequireUser]
        [HttpGet("clients")]
        public async Task<IActionResult> GetClientsByTrainer()
        {
            var clients = await _mediator.Send(
                new GetClientsByTrainerIdQuery());

            return Ok(clients);
        }
    }
}



