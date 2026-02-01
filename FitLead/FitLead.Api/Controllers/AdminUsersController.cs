using FitLead.Application.Users.Queries;
using FitLead.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    public sealed class AdminUsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminUsersController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
