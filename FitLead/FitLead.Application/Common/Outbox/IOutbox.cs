namespace FitLead.Application.Common.Outbox
{
    public interface IOutbox
    {
        Task EnqueueAsync<TPayload>(
            string type,
            TPayload payload,
            DateTime occurredAtUtc,
            CancellationToken cancellationToken);
    }
}
