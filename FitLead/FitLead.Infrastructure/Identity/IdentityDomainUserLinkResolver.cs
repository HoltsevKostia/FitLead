using FitLead.Application.Identity;
using FitLead.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Identity
{
    public sealed class IdentityDomainUserLinkResolver : IIdentityDomainUserLinkResolver
    {
        private readonly FitLeadDbContext _dbContext;

        public IdentityDomainUserLinkResolver(FitLeadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid?> ResolveDomainUserIdAsync(
            string identityUserId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.UserIdentityLinks
                .AsNoTracking()
                .Where(x => x.IdentityUserId == identityUserId)
                .Select(x => (Guid?)x.DomainUserId)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
