using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class NotificationMarkReadTests : NotificationTestBase
{
    public NotificationMarkReadTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task MarkRead_ShouldMarkOwnNotificationAsRead()
    {
        var client = await Users.RegisterClientAsync("notifications-mark-client");
        var notification = await CreateNotificationAsync(client.Id);
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.MarkReadAsync(notification.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var persisted = await Db.QueryAsync(async context =>
            await context.Notifications
                .AsNoTracking()
                .SingleAsync(candidate => candidate.Id == notification.Id));
        persisted.IsRead.Should().BeTrue();
        persisted.ReadAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkRead_ForAnotherUserNotification_ShouldReturnNotFound()
    {
        var client = await Users.RegisterClientAsync("notifications-mark-owner");
        var otherClient = await Users.RegisterClientAsync("notifications-mark-other");
        var notification = await CreateNotificationAsync(otherClient.Id);
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.MarkReadAsync(notification.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("notification.not_found");
    }

    [Fact]
    public async Task MarkAllRead_ShouldMarkOnlyCurrentUserNotificationsAsRead()
    {
        var client = await Users.RegisterClientAsync("notifications-mark-all-client");
        var otherClient = await Users.RegisterClientAsync("notifications-mark-all-other");
        var ownNotification = await CreateNotificationAsync(client.Id, title: "Own");
        var otherNotification = await CreateNotificationAsync(otherClient.Id, title: "Other");
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.MarkAllReadAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var persisted = await Db.QueryAsync(async context =>
            await context.Notifications
                .AsNoTracking()
                .Where(candidate => candidate.Id == ownNotification.Id ||
                                    candidate.Id == otherNotification.Id)
                .ToListAsync());
        persisted.Single(candidate => candidate.Id == ownNotification.Id).IsRead.Should().BeTrue();
        persisted.Single(candidate => candidate.Id == otherNotification.Id).IsRead.Should().BeFalse();
    }
}
