namespace FitLead.Application.Messenger.VideoReports.Outbox
{
    public sealed record VideoReportSubmittedOutboxPayload(
        Guid ChatId,
        Guid ReportId,
        Guid ClientId,
        string ClientName,
        Guid TrainerId,
        string Title,
        DateTime SubmittedAtUtc);
}
