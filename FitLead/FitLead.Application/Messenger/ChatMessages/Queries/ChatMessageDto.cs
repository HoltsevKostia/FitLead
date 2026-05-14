namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed record ChatMessageDto(
        Guid Id,
        Guid ChatId,
        Guid SenderId,
        string Type,
        string? Text,
        DateTime CreatedAtUtc);
}
