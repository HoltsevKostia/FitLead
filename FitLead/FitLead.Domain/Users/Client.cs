using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Users
{
    public sealed class Client : User
    {
        private Client() { }

        public Client(Guid id, string email, string fullName)
        : base(id, email, fullName, UserRole.Client)
        {
        }
    }
}
