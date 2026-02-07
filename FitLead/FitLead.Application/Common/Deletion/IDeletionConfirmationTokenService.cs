namespace FitLead.Application.Common.Deletion
{
    public interface IDeletionConfirmationTokenService
    {
        string IssueToken(DeletionScope scope, Guid targetId, int usageCount, DateTime utcNow);

        bool TryValidateToken(
            string token,
            DeletionScope expectedScope,
            Guid expectedTargetId,
            DateTime utcNow,
            out DeletionTokenPayload payload);
    }
}
