using FitLead.Application.Users.Commands.AssignClientToTrainer;
using FitLead.Application.Users.Commands.CreateUser;
using FitLead.Application.Users.Queries;
using FitLead.Domain.Users;
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

        [HttpPost("{trainerId:guid}/clients/{clientId:guid}")]
        public async Task<IActionResult> AssignClient(
            Guid trainerId,
            Guid clientId)
        {
            var result = await _mediator.Send(
                new AssignClientToTrainerCommand(trainerId, clientId));

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _mediator.Send(
                new GetAllUsersQuery());

            return Ok(users);
        }

        [HttpGet("trainers")]
        public async Task<IActionResult> GetTrainers()
        {
            var trainers = await _mediator.Send(
                new GetUsersByRoleQuery(UserRole.Trainer));

            return Ok(trainers);
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _mediator.Send(
                new GetUsersByRoleQuery(UserRole.Client));

            return Ok(clients);
        }

        [HttpGet("{trainerId:guid}/clients")]
        public async Task<IActionResult> GetClientsByTrainer(Guid trainerId)
        {
            var clients = await _mediator.Send(
                new GetClientsByTrainerIdQuery(trainerId));

            return Ok(clients);
        }
    }
}
