namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed record ChatDto(
        Guid Id,
        Guid TrainerId,
        Guid ClientId,
        DateTime CreatedAtUtc,
        DateTime? LastMessageAtUtc);
}
