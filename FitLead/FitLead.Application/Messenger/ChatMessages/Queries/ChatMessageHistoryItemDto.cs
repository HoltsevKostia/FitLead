namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed record ChatMessageHistoryItemDto(
        Guid Id,
        Guid ChatId,
        Guid SenderId,
        string SenderName,
        string Type,
        string? Text,
        DateTime CreatedAtUtc);
}
