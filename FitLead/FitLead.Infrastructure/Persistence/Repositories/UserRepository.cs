using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly FitLeadDbContext _context;

        public UserRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.DomainUsers
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(
            User user,
            CancellationToken cancellationToken)
        {
            await _context.DomainUsers.AddAsync(user, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            return await _context.Users
                .AnyAsync(x => x.Email == email, cancellationToken);
        }
    }
}
