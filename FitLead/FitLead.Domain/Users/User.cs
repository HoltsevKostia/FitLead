using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Users
{
    public sealed class User : AggregateRoot<Guid>
    {
        public string Email { get; private set; } = null!;
        public string FullName { get; private set; } = null!;
        public UserRole Role { get; private set; }

        private User() { } // EF

        private User(Guid id, string email, string fullName, UserRole role)
        {
            Id = id;
            Email = email;
            FullName = fullName;
            Role = role;
        }

        public static Result<User> CreateTrainer(string email, string fullName)
            => Create(email, fullName, UserRole.Trainer);

        public static Result<User> CreateClient(string email, string fullName)
            => Create(email, fullName, UserRole.Client);

        private static Result<User> Create(string email, string fullName, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<User>.Failure(
                    Error.Validation("user.create.email_required", "Email is required"));

            if (string.IsNullOrWhiteSpace(fullName))
                return Result<User>.Failure(
                    Error.Validation("user.create.full_name_required", "Full name is required"));

            return Result<User>.Success(
                new User(
                    Guid.NewGuid(),
                    email.Trim(),
                    fullName.Trim(),
                    role));
        }

        public bool IsTrainer => Role == UserRole.Trainer;
        public bool IsClient => Role == UserRole.Client;
    }
}
