using FitLead.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Queries
{
    public sealed class UserDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = null!;
        public string FullName { get; init; } = null!;
        public UserRole Role { get; init; }
    }
}
