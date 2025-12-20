using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Queries;
using FitLead.Domain.Users;
using FitLead.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace FitLead.Infrastructure.Repositories
{
    public sealed class UserReadRepository : IUserReadRepository
    {
        private readonly FitLeadDbContext _context;

        public UserReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<UserDto>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await _context.Users
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                    Role = x.Role
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<UserDto>> GetByRoleAsync(
            UserRole role,
            CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(x => x.Role == role)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                    Role = x.Role
                })
                .ToListAsync(cancellationToken);
        }
    }
}
