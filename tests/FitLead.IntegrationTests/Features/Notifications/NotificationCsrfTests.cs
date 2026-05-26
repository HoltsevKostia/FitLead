using System.Net;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class NotificationCsrfTests : NotificationTestBase
{
    public NotificationCsrfTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task MarkRead_WithoutCsrf_ShouldReturnBadRequest()
    {
        var client = await Users.RegisterClientAsync("notifications-csrf-read");
        var notification = await CreateNotificationAsync(client.Id);
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.MarkReadAsync(
            notification.Id,
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MarkAllRead_WithoutCsrf_ShouldReturnBadRequest()
    {
        var client = await Users.RegisterClientAsync("notifications-csrf-all");
        await CreateNotificationAsync(client.Id);
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.MarkAllReadAsync(includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNotifications_WithoutCsrf_ShouldBeAllowed()
    {
        var client = await Users.RegisterClientAsync("notifications-csrf-get");
        await CreateNotificationAsync(client.Id);
        var notificationsClient = await Api.NotificationsAsync(client.Auth);

        var response = await notificationsClient.GetNotificationsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
