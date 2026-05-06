using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Modules.Users;

namespace FitLead.Infrastructure.Modules.Users
{
    public sealed class UsersModule : IUsersModule
    {
        private readonly IUserRepository _userRepository;
        private readonly ITrainerClientRepository _trainerClientRepository;

        public UsersModule(
            IUserRepository userRepository,
            ITrainerClientRepository trainerClientRepository)
        {
            _userRepository = userRepository;
            _trainerClientRepository = trainerClientRepository;
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

        public Task<Guid?> GetActiveTrainerIdForClientAsync(
            Guid clientId,
            CancellationToken cancellationToken = default)
        {
            return _trainerClientRepository.GetTrainerIdByClientIdAsync(
                clientId,
                cancellationToken);
        }

        public async Task EnsureTrainerClientRelationshipAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken = default)
        {
            var exists = await _trainerClientRepository.ExistsAsync(
                trainerId,
                clientId,
                cancellationToken);

            if (exists)
            {
                return;
            }

            await _trainerClientRepository.AddAsync(
                trainerId,
                clientId,
                cancellationToken);
        }
    }
}
