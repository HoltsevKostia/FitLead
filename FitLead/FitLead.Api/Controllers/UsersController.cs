using FitLead.Application.Users.CreateUser;
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
    }
}
