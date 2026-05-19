namespace FitLead.Domain.Outbox
{
    public enum OutboxMessageStatus
    {
        Pending = 1,
        Processed = 2,
        Failed = 3
    }
}
