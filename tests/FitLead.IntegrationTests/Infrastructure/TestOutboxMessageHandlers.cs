using FitLead.Application.Common.Outbox;
using FitLead.Application.Notifications.Queries;
using FitLead.Application.Notifications.Realtime;
using FitLead.Domain.Outbox;
using System.Collections.Concurrent;

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

public sealed class TestNotificationRealtimeNotifier : INotificationRealtimeNotifier
{
    private readonly ConcurrentBag<NotificationDto> _notifications = new();

    public IReadOnlyCollection<NotificationDto> Notifications => _notifications.ToArray();

    public Task NotificationCreatedAsync(
        NotificationDto notification,
        CancellationToken cancellationToken)
    {
        _notifications.Add(notification);
        return Task.CompletedTask;
    }
}
