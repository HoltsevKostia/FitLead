using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetAllUsersQuery(
    ) : IRequest<Result<IReadOnlyList<UserDto>>>;
}

