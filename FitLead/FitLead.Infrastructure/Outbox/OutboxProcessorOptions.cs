namespace FitLead.Infrastructure.Outbox
{
    public sealed class OutboxProcessorOptions
    {
        public const string SectionName = "OutboxProcessor";

        public int BatchSize { get; init; } = 20;
        public int PollingIntervalSeconds { get; init; } = 5;
        public int MaxAttempts { get; init; } = 4;
    }
}
