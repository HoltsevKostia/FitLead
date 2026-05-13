namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed record ChatDto(
        Guid Id,
        Guid TrainerId,
        Guid ClientId,
        string TrainerName,
        string ClientName,
        DateTime CreatedAtUtc,
        DateTime? LastMessageAtUtc);
}
