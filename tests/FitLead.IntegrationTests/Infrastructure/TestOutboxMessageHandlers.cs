using FitLead.Application.Common.Outbox;
using FitLead.Domain.Outbox;

namespace FitLead.IntegrationTests.Infrastructure;

public static class TestOutboxMessageTypes
{
    public const string Success = "Testing.OutboxSuccess";
    public const string Failure = "Testing.OutboxFailure";
}

public sealed class SuccessfulTestOutboxMessageHandler : IOutboxMessageHandler
{
    public string Type => TestOutboxMessageTypes.Success;

    public Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public sealed class FailingTestOutboxMessageHandler : IOutboxMessageHandler
{
    public string Type => TestOutboxMessageTypes.Failure;

    public Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Test outbox handler failure.");
    }
}
