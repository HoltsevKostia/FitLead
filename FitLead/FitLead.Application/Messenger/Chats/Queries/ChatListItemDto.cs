namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed record ChatListItemDto(
        Guid Id,
        Guid TrainerId,
        string TrainerName,
        Guid ClientId,
        string ClientName,
        DateTime? LastMessageAtUtc);
}
