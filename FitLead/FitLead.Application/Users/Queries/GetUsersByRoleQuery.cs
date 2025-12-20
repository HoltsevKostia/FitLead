using FitLead.Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetUsersByRoleQuery(
        UserRole Role
    ) : IRequest<IReadOnlyList<UserDto>>;
}
