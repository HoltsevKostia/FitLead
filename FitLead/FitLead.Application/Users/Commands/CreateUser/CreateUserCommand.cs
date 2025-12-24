using FitLead.Application.Common;
using FitLead.Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Commands.CreateUser
{
    public sealed record CreateUserCommand(
    string Email,
    string FullName,
    UserRole Role
) : IRequest<Result<Guid>>;
}
