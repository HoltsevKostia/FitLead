namespace FitLead.Application.Messenger.VideoReports.Outbox
{
    public sealed record VideoReportReviewedOutboxPayload(
        Guid ChatId,
        Guid ReportId,
        Guid ClientId,
        Guid TrainerId,
        string Title,
        DateTime ReviewedAtUtc);
}
