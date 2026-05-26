using System.Net;
using FitLead.Application.Notifications.Queries;
using FitLead.Domain.Notifications;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class NotificationReadTests : NotificationTestBase
{
    public NotificationReadTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetNotifications_ShouldReturnOnlyCurrentUserNotifications()
    {
        var client = await Users.RegisterClientAsync("notifications-read-client");
        var otherClient = await Users.RegisterClientAsync("notifications-read-other");
        var ownNotification = await CreateNotificationAsync(
            client.Id,
            title: "Own notification",
            createdAtUtc: DateTime.UtcNow.AddMinutes(-1));
        await CreateNotificationAsync(
            otherClient.Id,
            title: "Other notification",
            createdAtUtc: DateTime.UtcNow);
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.GetNotificationsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.ReadRequiredJsonAsync<IReadOnlyList<NotificationDto>>();
        notifications.Should().ContainSingle();
        notifications[0].Id.Should().Be(ownNotification.Id);
        notifications[0].Title.Should().Be("Own notification");
        notifications[0].Type.Should().Be(NotificationType.VideoReportSubmitted.ToString());
        notifications[0].IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnCurrentUserUnreadCount()
    {
        var client = await Users.RegisterClientAsync("notifications-count-client");
        var otherClient = await Users.RegisterClientAsync("notifications-count-other");
        await CreateNotificationAsync(client.Id, title: "Unread one");
        await CreateNotificationAsync(client.Id, title: "Unread two");
        var readNotification = await CreateNotificationAsync(client.Id, title: "Read");
        readNotification.MarkRead(DateTime.UtcNow);
        await Db.ExecuteAsync(async context =>
        {
            context.Notifications.Update(readNotification);
            await context.SaveChangesAsync();
        });
        await CreateNotificationAsync(otherClient.Id, title: "Other unread");
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.GetUnreadCountAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var count = await response.ReadRequiredJsonAsync<UnreadNotificationCountDto>();
        count.Count.Should().Be(2);
    }
}
