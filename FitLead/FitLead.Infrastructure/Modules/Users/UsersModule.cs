using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Modules.Users;

namespace FitLead.Infrastructure.Modules.Users
{
    public sealed class UsersModule : IUsersModule
    {
        private readonly IUserRepository _userRepository;

        public UsersModule(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserModuleDescriptor?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
                return null;

            return new UserModuleDescriptor(user.Id, user.Role);
        }

        public async Task<bool> IsInRoleAsync(
            Guid userId,
            Domain.Users.UserRole role,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            return user is not null && user.Role == role;
        }
    }
}
