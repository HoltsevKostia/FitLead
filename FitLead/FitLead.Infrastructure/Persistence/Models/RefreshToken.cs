namespace FitLead.Infrastructure.Persistence.Models
{
    public sealed class RefreshToken
    {
        public Guid Id { get; private set; }
        public string IdentityUserId { get; private set; } = null!;
        public string TokenHash { get; private set; } = null!;
        public Guid FamilyId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }
        public Guid? ReplacedByTokenId { get; private set; }
        public string? ReasonRevoked { get; private set; }

        private RefreshToken() { }

        public RefreshToken(
            string identityUserId,
            string tokenHash,
            Guid familyId,
            DateTime createdAtUtc,
            DateTime expiresAtUtc)
        {
            Id = Guid.NewGuid();
            IdentityUserId = identityUserId;
            TokenHash = tokenHash;
            FamilyId = familyId;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }
    }
}
