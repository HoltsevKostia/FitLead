namespace FitLead.Application.Media.MediaAssets.Queries
{
    public sealed record MediaAssetPreviewDto(
        Guid Id,
        string DeliveryUrl,
        string? FileName,
        string ContentType,
        long SizeBytes,
        string Kind,
        int? DurationSeconds);
}
