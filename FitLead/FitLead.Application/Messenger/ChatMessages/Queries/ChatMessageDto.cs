namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed record ChatMessageDto(
        Guid Id,
        Guid ChatId,
        Guid SenderId,
        string SenderName,
        string Type,
        string? Text,
        Guid? VideoReportId,
        DateTime CreatedAtUtc);
}
