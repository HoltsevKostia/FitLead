namespace FitLead.Application.Messenger.VideoReports.Outbox
{
    public sealed record VideoReportSubmittedOutboxPayload(
        Guid ChatId,
        Guid ReportId,
        Guid ClientId,
        Guid TrainerId,
        string Title,
        DateTime SubmittedAtUtc);
}
