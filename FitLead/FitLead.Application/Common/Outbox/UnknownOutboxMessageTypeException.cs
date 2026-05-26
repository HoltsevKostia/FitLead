namespace FitLead.Application.Common.Outbox
{
    public sealed class UnknownOutboxMessageTypeException : Exception
    {
        public UnknownOutboxMessageTypeException(string type)
            : base($"Unknown outbox message type '{type}'.")
        {
            Type = type;
        }

        public string Type { get; }
    }
}
