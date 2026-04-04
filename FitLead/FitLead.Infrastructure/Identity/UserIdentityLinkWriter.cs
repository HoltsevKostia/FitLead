using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Identity
{
    public sealed class UserIdentityLinkWriter : IUserIdentityLinkWriter
    {
        private readonly FitLeadDbContext _dbContext;

        public UserIdentityLinkWriter(FitLeadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> AddAsync(
            Guid domainUserId,
            string identityUserId,
            CancellationToken cancellationToken)
        {
            var identityAlreadyLinked = await _dbContext.UserIdentityLinks
                .AnyAsync(x => x.IdentityUserId == identityUserId, cancellationToken);
            if (identityAlreadyLinked)
            {
                return Result.Failure(
                    Error.Conflict("auth.identity_already_linked", "Identity user is already linked"));
            }

            var domainAlreadyLinked = await _dbContext.UserIdentityLinks
                .AnyAsync(x => x.DomainUserId == domainUserId, cancellationToken);
            if (domainAlreadyLinked)
            {
                return Result.Failure(
                    Error.Conflict("auth.domain_user_already_linked", "Domain user is already linked"));
            }

            await _dbContext.UserIdentityLinks.AddAsync(
                new UserIdentityLink(domainUserId, identityUserId),
                cancellationToken);

            return Result.Success();
        }
    }
}
