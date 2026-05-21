using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Notifications.Outbox;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class NotificationRealtimeOutboxTests : NotificationTestBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public NotificationRealtimeOutboxTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ProcessNotificationCreated_ShouldSendNotificationToRealtimeNotifier()
    {
        var client = await Users.RegisterClientAsync("notif-realtime-client");
        var notification = await CreateNotificationAsync(client.Id);
        var message = await AddOutboxMessageAsync(
            OutboxEventTypes.Notifications.Created,
            new NotificationCreatedOutboxPayload(
                notification.Id,
                client.Id,
                notification.CreatedAtUtc));

        await ProcessAsync(message.Id);

        var notifier = Fixture.Factory.Services.GetRequiredService<TestNotificationRealtimeNotifier>();
        notifier.Notifications.Should().ContainSingle(delivered =>
            delivered.Id == notification.Id &&
            delivered.RecipientUserId == client.Id &&
            delivered.Title == notification.Title &&
            delivered.LinkUrl == notification.LinkUrl);
    }

    private async Task<OutboxMessage> AddOutboxMessageAsync<TPayload>(
        string type,
        TPayload payload)
    {
        var serializedPayload = JsonSerializer.Serialize(payload, SerializerOptions);
        var message = OutboxMessage.Create(
            type,
            serializedPayload,
            DateTime.UtcNow).Value;

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await repository.AddAsync(message, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        return message;
    }

    private async Task ProcessAsync(Guid messageId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
        await processor.ProcessAsync(messageId, CancellationToken.None);
    }
}
