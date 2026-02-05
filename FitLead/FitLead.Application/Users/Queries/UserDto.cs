using FitLead.Domain.Users;

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
