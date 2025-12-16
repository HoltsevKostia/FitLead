using FitLead.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Users
{
    public abstract class User : AggregateRoot<Guid>
    {
        public string Email { get; protected set; } = null!;
        public string FullName { get; protected set; } = null!;
        public UserRole Role { get; protected set; }

        protected User() { }

        protected User(Guid id, string email, string fullName, UserRole role)
        {
            Id = id;
            Email = email;
            FullName = fullName;
            Role = role;
        }
    }
}
