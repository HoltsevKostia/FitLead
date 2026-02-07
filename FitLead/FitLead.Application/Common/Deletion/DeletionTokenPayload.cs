namespace FitLead.Application.Common.Deletion
{
    public sealed record DeletionTokenPayload(
        DeletionScope Scope,
        Guid TargetId,
        int UsageCount,
        DateTime IssuedAtUtc);
}
