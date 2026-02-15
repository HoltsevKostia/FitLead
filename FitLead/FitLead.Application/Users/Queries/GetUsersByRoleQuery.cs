using FitLead.Domain.Users;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetUsersByRoleQuery(
        UserRole Role
    ) : IRequest<Result<IReadOnlyList<UserDto>>>;
}

