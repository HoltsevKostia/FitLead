namespace FitLead.Application.Common.Outbox
{
    public interface IOutboxMessageProcessor
    {
        Task ProcessAsync(
            Guid messageId,
            CancellationToken cancellationToken);
    }
}
