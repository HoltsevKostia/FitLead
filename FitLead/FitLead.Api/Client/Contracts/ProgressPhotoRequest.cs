namespace FitLead.Api.Client.Contracts
{
    public sealed record ProgressPhotoRequest(
        Guid MediaAssetId,
        DateOnly TakenAt,
        string? Label,
        string? Note);
}
