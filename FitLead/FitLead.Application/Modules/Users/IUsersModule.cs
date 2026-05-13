using FitLead.Domain.Users;

namespace FitLead.Application.Modules.Users
{
    public interface IUsersModule
    {
        Task<UserModuleDescriptor?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<bool> IsInRoleAsync(
            Guid userId,
            UserRole role,
            CancellationToken cancellationToken = default);

        Task<Guid?> GetActiveTrainerIdForClientAsync(
            Guid clientId,
            CancellationToken cancellationToken = default);

        Task<bool> HasTrainerClientRelationshipAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken = default);

        Task EnsureTrainerClientRelationshipAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken = default);

        Task<TrainerPublicProfileDescriptor?> GetTrainerPublicProfileAsync(
            Guid trainerId,
            CancellationToken cancellationToken = default);
    }
}
