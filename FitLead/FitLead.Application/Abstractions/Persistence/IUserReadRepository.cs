using FitLead.Application.Users.Queries;
using FitLead.Domain.Users;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IUserReadRepository
    {
        Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<UserDto>> GetByRoleAsync(
            UserRole role,
            CancellationToken cancellationToken);
    }
}
