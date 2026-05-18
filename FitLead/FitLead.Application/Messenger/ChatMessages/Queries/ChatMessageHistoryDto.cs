namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed record ChatMessageHistoryDto(
        IReadOnlyList<ChatMessageDto> Items,
        bool HasMore);
}
