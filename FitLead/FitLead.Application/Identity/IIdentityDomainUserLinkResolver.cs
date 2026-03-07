namespace FitLead.Application.Identity
{
    public interface IIdentityDomainUserLinkResolver
    {
        Task<Guid?> ResolveDomainUserIdAsync(
            string identityUserId,
            CancellationToken cancellationToken);
    }
}
