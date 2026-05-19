namespace FitLead.Api.Media.Contracts
{
    public sealed record RegisterMediaAssetRequest(
        string StorageProvider,
        string StorageObjectId,
        string DeliveryUrl,
        string? FileName,
        string ContentType,
        long SizeBytes,
        string Kind,
        int? DurationSeconds);
}
