using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Identity
{
    public sealed record RegisterUserCommand(
        string Email,
        string Password,
        string FullName,
        string Role) : IRequest<Result<AuthTokensResult>>;
}
