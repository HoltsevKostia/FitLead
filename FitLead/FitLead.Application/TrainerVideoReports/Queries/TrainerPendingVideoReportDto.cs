namespace FitLead.Application.TrainerVideoReports.Queries
{
    public sealed record TrainerPendingVideoReportDto(
        Guid ReportId,
        Guid ChatId,
        Guid ClientId,
        string ClientName,
        string Title,
        string? Description,
        int MediaCount,
        DateTime CreatedAtUtc);
}
