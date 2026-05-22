using FitLead.Application.Common.Outbox;
using FitLead.Application.Notifications.Push;
using FitLead.Application.Notifications.Queries;
using FitLead.Application.Notifications.Realtime;
using FitLead.Domain.Notifications.PushSubscriptions;
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

public sealed class TestWebPushSender : IWebPushSender
{
    private readonly ConcurrentBag<TestWebPushDelivery> _deliveries = new();
    private readonly ConcurrentDictionary<Guid, WebPushSendResult> _resultsBySubscriptionId = new();
    private readonly ConcurrentDictionary<Guid, Exception> _exceptionsBySubscriptionId = new();

    public IReadOnlyCollection<TestWebPushDelivery> Deliveries => _deliveries.ToArray();

    public void SetResult(
        Guid subscriptionId,
        WebPushSendResult result)
    {
        _resultsBySubscriptionId[subscriptionId] = result;
    }

    public void SetException(
        Guid subscriptionId,
        Exception exception)
    {
        _exceptionsBySubscriptionId[subscriptionId] = exception;
    }

    public Task<WebPushSendResult> SendAsync(
        PushSubscription subscription,
        WebPushNotification notification,
        CancellationToken cancellationToken)
    {
        if (_exceptionsBySubscriptionId.TryGetValue(subscription.Id, out var exception))
        {
            throw exception;
        }

        _deliveries.Add(new TestWebPushDelivery(
            subscription.Id,
            subscription.UserId,
            notification));

        return Task.FromResult(
            _resultsBySubscriptionId.GetValueOrDefault(
                subscription.Id,
                WebPushSendResult.Sent));
    }
}

public sealed record TestWebPushDelivery(
    Guid SubscriptionId,
    Guid UserId,
    WebPushNotification Notification);
