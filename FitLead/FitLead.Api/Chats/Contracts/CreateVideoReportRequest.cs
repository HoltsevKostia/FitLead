namespace FitLead.Api.Chats.Contracts
{
    public sealed record CreateVideoReportRequest(
        string Title,
        string? Description,
        IReadOnlyList<Guid> MediaAssetIds);
}
