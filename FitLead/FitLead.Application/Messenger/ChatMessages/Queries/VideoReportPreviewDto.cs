namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed record VideoReportPreviewDto(
        Guid Id,
        string Title,
        string? Description,
        string Status,
        int MediaCount);
}
