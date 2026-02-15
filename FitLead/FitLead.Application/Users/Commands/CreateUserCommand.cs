using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Users.Commands
{
    public sealed record CreateUserCommand(
    string Email,
    string FullName,
    UserRole Role
) : IRequest<Result<Guid>>;
}
