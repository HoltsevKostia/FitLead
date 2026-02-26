namespace FitLead.Infrastructure.Persistence.Models
{
    public sealed class UserIdentityLink
    {
        public Guid DomainUserId { get; private set; }
        public string IdentityUserId { get; private set; } = null!;
        public DateTime CreatedAtUtc { get; private set; }

        private UserIdentityLink() { }

        public UserIdentityLink(Guid domainUserId, string identityUserId)
        {
            DomainUserId = domainUserId;
            IdentityUserId = identityUserId;
            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}
