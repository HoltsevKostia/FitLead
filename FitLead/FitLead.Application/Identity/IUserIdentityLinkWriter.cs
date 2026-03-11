using FitLead.Common.Results;

namespace FitLead.Application.Identity
{
    public interface IUserIdentityLinkWriter
    {
        Task<Result> AddAsync(
            Guid domainUserId,
            string identityUserId,
            CancellationToken cancellationToken);
    }
}
