namespace FitLead.Application.Messenger.VideoReports.Queries
{
    public sealed record VideoReportMediaDto(
        Guid Id,
        string DeliveryUrl,
        string? FileName,
        string ContentType,
        long SizeBytes,
        string Kind,
        int? DurationSeconds,
        int OrderInReport);
}
