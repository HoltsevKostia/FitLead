namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientLastVideoReportDto(
        Guid ReportId,
        Guid ChatId,
        string Title,
        string? Description,
        string Status,
        int MediaCount,
        DateTime CreatedAtUtc,
        DateTime? ReviewedAtUtc);
}
