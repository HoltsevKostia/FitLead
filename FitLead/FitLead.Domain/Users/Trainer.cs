using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Users
{
    public sealed class Trainer : User
    {
        private readonly List<Client> _clients = new();

        public IReadOnlyCollection<Client> Clients => _clients.AsReadOnly();

        private Trainer() { }

        public Trainer(Guid id, string email, string fullName)
        : base(id, email, fullName, UserRole.Trainer)
        {
        }

        public void AddClient(Client client)
        {
            if (_clients.Any(c => c.Id == client.Id))
                throw new InvalidOperationException("Client already assigned to trainer");

            _clients.Add(client);
        }
    }
}
