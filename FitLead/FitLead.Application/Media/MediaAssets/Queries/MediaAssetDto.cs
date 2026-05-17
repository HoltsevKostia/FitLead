namespace FitLead.Application.Media.MediaAssets.Queries
{
    public sealed record MediaAssetDto(
        Guid Id,
        string StorageProvider,
        string StorageObjectId,
        string DeliveryUrl,
        string? FileName,
        string ContentType,
        long SizeBytes,
        string Kind,
        int? DurationSeconds,
        string Status,
        DateTime CreatedAtUtc);
}
