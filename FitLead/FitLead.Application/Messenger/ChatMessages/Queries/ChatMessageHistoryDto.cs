namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed record ChatMessageHistoryDto(
        IReadOnlyList<ChatMessageHistoryItemDto> Items,
        bool HasMore);
}
