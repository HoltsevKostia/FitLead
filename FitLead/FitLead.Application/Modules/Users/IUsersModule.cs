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
    }
}
