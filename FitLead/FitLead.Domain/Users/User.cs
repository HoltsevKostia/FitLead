using FitLead.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Users
{
    public sealed class User : AggregateRoot<Guid>
    {
        private readonly List<Guid> _clientIds = new();

        public string Email { get; private set; } = null!;
        public string FullName { get; private set; } = null!;
        public UserRole Role { get; private set; }

        public IReadOnlyCollection<Guid> ClientIds => _clientIds.AsReadOnly();

        private User() { } // EF

        private User(Guid id, string email, string fullName, UserRole role)
        {
            Id = id;
            Email = email;
            FullName = fullName;
            Role = role;
        }

        public static User CreateTrainer(string email, string fullName)
            => Create(email, fullName, UserRole.Trainer);

        public static User CreateClient(string email, string fullName)
            => Create(email, fullName, UserRole.Client);

        private static User Create(string email, string fullName, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required");

            return new User(
                Guid.NewGuid(),
                email.Trim(),
                fullName.Trim(),
                role);
        }

        public void AssignClient(Guid clientId)
        {
            if (Role != UserRole.Trainer)
                throw new InvalidOperationException("Only trainer can have clients");

            if (_clientIds.Contains(clientId))
                throw new InvalidOperationException("Client already assigned");

            _clientIds.Add(clientId);
        }
    }
}
