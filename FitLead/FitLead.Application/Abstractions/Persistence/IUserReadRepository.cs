using FitLead.Application.Users.Queries;
using FitLead.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
