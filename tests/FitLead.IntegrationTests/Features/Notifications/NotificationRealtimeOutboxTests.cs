using FitLead.Application.Common.Outbox;
using FitLead.Application.Notifications.Outbox;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class NotificationRealtimeOutboxTests : NotificationTestBase
{
    public NotificationRealtimeOutboxTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ProcessNotificationCreated_ShouldSendNotificationToRealtimeNotifier()
    {
        var client = await Users.RegisterClientAsync("notif-realtime-client");
        var notification = await CreateNotificationAsync(client.Id);
        var message = await AddOutboxMessageForProcessingAsync(
            OutboxEventTypes.Notifications.Created,
            new NotificationCreatedOutboxPayload(
                notification.Id,
                client.Id,
                notification.CreatedAtUtc));

        await ProcessOutboxMessageAsync(message.Id);

        var notifier = Fixture.Factory.Services.GetRequiredService<TestNotificationRealtimeNotifier>();
        notifier.Notifications.Should().ContainSingle(delivered =>
            delivered.Id == notification.Id &&
            delivered.RecipientUserId == client.Id &&
            delivered.Title == notification.Title &&
            delivered.LinkUrl == notification.LinkUrl);
    }
}
