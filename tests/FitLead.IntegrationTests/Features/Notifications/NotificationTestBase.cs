using FitLead.Domain.Notifications;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;

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
}
