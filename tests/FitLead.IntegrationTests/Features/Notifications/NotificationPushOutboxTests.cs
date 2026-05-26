using FitLead.Application.Common.Outbox;
using FitLead.Application.Notifications.Outbox;
using FitLead.Application.Notifications.Push;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class NotificationPushOutboxTests : NotificationTestBase
{
    public NotificationPushOutboxTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ProcessNotificationCreated_WithActiveSubscription_ShouldSendWebPushAndMarkSubscriptionUsed()
    {
        var client = await Users.RegisterClientAsync("notif-push-client");
        var notification = await CreateNotificationAsync(client.Id);
        var subscription = await CreatePushSubscriptionAsync(client.Id);
        var message = await AddNotificationCreatedOutboxMessageAsync(notification.Id, client.Id, notification.CreatedAtUtc);

        await ProcessOutboxMessageAsync(message.Id);

        var sender = Fixture.Factory.Services.GetRequiredService<TestWebPushSender>();
        sender.Deliveries.Should().ContainSingle(delivery =>
            delivery.SubscriptionId == subscription.Id &&
            delivery.UserId == client.Id &&
            delivery.Notification.Url == notification.LinkUrl);

        var persistedSubscription = await GetSubscriptionAsync(subscription.Id);
        persistedSubscription.LastUsedAtUtc.Should().NotBeNull();
        persistedSubscription.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task ProcessNotificationCreated_WhenSubscriptionExpired_ShouldRevokeSubscription()
    {
        var client = await Users.RegisterClientAsync("notif-push-expired-client");
        var notification = await CreateNotificationAsync(client.Id);
        var subscription = await CreatePushSubscriptionAsync(client.Id);
        var sender = Fixture.Factory.Services.GetRequiredService<TestWebPushSender>();
        sender.SetResult(subscription.Id, WebPushSendResult.SubscriptionExpired);
        var message = await AddNotificationCreatedOutboxMessageAsync(notification.Id, client.Id, notification.CreatedAtUtc);

        await ProcessOutboxMessageAsync(message.Id);

        var persistedSubscription = await GetSubscriptionAsync(subscription.Id);
        persistedSubscription.RevokedAtUtc.Should().NotBeNull();
        persistedSubscription.LastUsedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task ProcessNotificationCreated_WhenWebPushFails_ShouldStillProcessOutboxMessage()
    {
        var client = await Users.RegisterClientAsync("notif-push-failure-client");
        var notification = await CreateNotificationAsync(client.Id);
        var subscription = await CreatePushSubscriptionAsync(client.Id);
        var sender = Fixture.Factory.Services.GetRequiredService<TestWebPushSender>();
        sender.SetException(subscription.Id, new InvalidOperationException("Push provider is unavailable."));
        var message = await AddNotificationCreatedOutboxMessageAsync(notification.Id, client.Id, notification.CreatedAtUtc);

        await ProcessOutboxMessageAsync(message.Id);

        var persistedSubscription = await GetSubscriptionAsync(subscription.Id);
        persistedSubscription.LastUsedAtUtc.Should().BeNull();
        persistedSubscription.RevokedAtUtc.Should().BeNull();

        var persistedMessage = await Db.QueryAsync(context =>
            context.OutboxMessages.SingleAsync(outboxMessage => outboxMessage.Id == message.Id));
        persistedMessage.Status.Should().Be(OutboxMessageStatus.Processed);
    }

    private Task<OutboxMessage> AddNotificationCreatedOutboxMessageAsync(
        Guid notificationId,
        Guid recipientUserId,
        DateTime createdAtUtc)
    {
        return AddOutboxMessageForProcessingAsync(
            OutboxEventTypes.Notifications.Created,
            new NotificationCreatedOutboxPayload(
                notificationId,
                recipientUserId,
                createdAtUtc));
    }

    private async Task<Domain.Notifications.PushSubscriptions.PushSubscription> GetSubscriptionAsync(Guid subscriptionId)
    {
        return await Db.QueryAsync(context =>
            context.PushSubscriptions.SingleAsync(subscription => subscription.Id == subscriptionId));
    }
}
