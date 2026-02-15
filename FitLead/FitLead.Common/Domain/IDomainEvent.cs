namespace FitLead.Common.Domain
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}
