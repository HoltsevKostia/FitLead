using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Outbox;
using FitLead.Domain.Outbox;
using FitLead.Domain.Notifications;
using FitLead.Domain.Notifications.PushSubscriptions;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Notifications;

public abstract class NotificationTestBase : IntegrationTestBase
{
    protected readonly TestDb Db;
    protected readonly TestOutbox Outbox;
    protected readonly TestUsers Users;
    protected readonly TestApiClients Api;

    protected NotificationTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Outbox = new TestOutbox(Db);
        Users = new TestUsers(fixture, Db);
        Api = new TestApiClients(fixture);
    }

    protected async Task<Notification> CreateNotificationAsync(
        Guid recipientUserId,
        NotificationType type = NotificationType.VideoReportSubmitted,
        string title = "Video report submitted",
        string? body = "Please review",
        string linkUrl = "/chats/00000000-0000-0000-0000-000000000001/reports/00000000-0000-0000-0000-000000000002",
        DateTime? createdAtUtc = null,
        Guid? sourceEventId = null)
    {
        var notification = Notification.Create(
            recipientUserId,
            type,
            title,
            body,
            linkUrl,
            createdAtUtc ?? DateTime.UtcNow,
            sourceEventId ?? Guid.NewGuid()).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.Notifications.AddAsync(notification);
            await context.SaveChangesAsync();
        });

        return notification;
    }

    protected async Task<PushSubscription> CreatePushSubscriptionAsync(
        Guid userId,
        string? endpoint = null,
        DateTime? createdAtUtc = null)
    {
        var subscription = PushSubscription.Create(
            userId,
            endpoint ?? $"https://push.example.com/{Guid.NewGuid():D}",
            "test-p256dh",
            "test-auth",
            "Test browser",
            createdAtUtc ?? DateTime.UtcNow).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.PushSubscriptions.AddAsync(subscription);
            await context.SaveChangesAsync();
        });

        return subscription;
    }

    protected async Task<OutboxMessage> AddOutboxMessageForProcessingAsync<TPayload>(
        string type,
        TPayload payload)
    {
        var serializedPayload = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
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

    protected async Task ProcessOutboxMessageAsync(Guid messageId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
        await processor.ProcessAsync(messageId, CancellationToken.None);
    }
}
