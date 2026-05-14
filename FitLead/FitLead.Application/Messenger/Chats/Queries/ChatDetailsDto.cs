namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed record ChatDetailsDto(
        Guid Id,
        Guid TrainerId,
        string TrainerName,
        Guid ClientId,
        string ClientName,
        DateTime CreatedAtUtc,
        DateTime? LastMessageAtUtc);
}
