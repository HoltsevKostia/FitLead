namespace FitLead.Application.Messenger.ChatMessages.Outbox
{
    public sealed record ChatMessageCreatedOutboxPayload(
        Guid ChatId,
        Guid MessageId);
}
