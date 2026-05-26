using System.Net;
using FitLead.Application.Notifications.Push;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class PushSubscriptionTests : NotificationTestBase
{
    public PushSubscriptionTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetVapidPublicKey_ShouldReturnConfiguredPublicKey()
    {
        var client = await Users.RegisterClientAsync("push-public-key-client");
        var pushClient = await Api.PushAsync(client.Auth);

        var response = await pushClient.GetVapidPublicKeyAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var publicKey = await response.ReadRequiredJsonAsync<VapidPublicKeyDto>();
        publicKey.PublicKey.Should().Be("test-vapid-public-key");
    }

    [Fact]
    public async Task RegisterSubscription_ShouldCreateSubscriptionForCurrentUser()
    {
        var client = await Users.RegisterClientAsync("push-register-client");
        var pushClient = await Api.PushAsync(client.Auth);
        var endpoint = $"https://push.example.com/{Guid.NewGuid():D}";

        var response = await pushClient.RegisterSubscriptionAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.ReadRequiredJsonAsync<PushSubscriptionDto>();
        created.Endpoint.Should().Be(endpoint);

        var subscription = await Db.QueryAsync(context =>
            context.PushSubscriptions.SingleAsync(x => x.Id == created.Id));
        subscription.UserId.Should().Be(client.Id);
        subscription.Endpoint.Should().Be(endpoint);
        subscription.P256dh.Should().Be("test-p256dh");
        subscription.Auth.Should().Be("test-auth");
        subscription.UserAgent.Should().Be("Test browser");
        subscription.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task RegisterSubscription_WithExistingEndpoint_ShouldRefreshExistingSubscription()
    {
        var firstClient = await Users.RegisterClientAsync("push-refresh-first-client");
        var secondClient = await Users.RegisterClientAsync("push-refresh-second-client");
        var firstPushClient = await Api.PushAsync(firstClient.Auth);
        var secondPushClient = await Api.PushAsync(secondClient.Auth);
        var endpoint = $"https://push.example.com/{Guid.NewGuid():D}";
        var firstResponse = await firstPushClient.RegisterSubscriptionAsync(endpoint);
        var created = await firstResponse.ReadRequiredJsonAsync<PushSubscriptionDto>();

        var secondResponse = await secondPushClient.RegisterSubscriptionAsync(
            endpoint,
            p256dh: "new-p256dh",
            auth: "new-auth",
            userAgent: "New browser");

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await secondResponse.ReadRequiredJsonAsync<PushSubscriptionDto>();
        refreshed.Id.Should().Be(created.Id);

        var subscriptions = await Db.QueryAsync(context =>
            context.PushSubscriptions.ToListAsync());
        subscriptions.Should().ContainSingle();
        var subscription = subscriptions.Single();
        subscription.UserId.Should().Be(secondClient.Id);
        subscription.P256dh.Should().Be("new-p256dh");
        subscription.Auth.Should().Be("new-auth");
        subscription.UserAgent.Should().Be("New browser");
        subscription.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task RegisterSubscription_WithoutCsrf_ShouldReturnBadRequest()
    {
        var client = await Users.RegisterClientAsync("push-csrf-client");
        var pushClient = await Api.PushAsync(client.Auth);

        var response = await pushClient.RegisterSubscriptionAsync(
            $"https://push.example.com/{Guid.NewGuid():D}",
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RevokeCurrentSubscription_ShouldRevokeOwnSubscription()
    {
        var client = await Users.RegisterClientAsync("push-revoke-client");
        var pushClient = await Api.PushAsync(client.Auth);
        var endpoint = $"https://push.example.com/{Guid.NewGuid():D}";
        var registerResponse = await pushClient.RegisterSubscriptionAsync(endpoint);
        var created = await registerResponse.ReadRequiredJsonAsync<PushSubscriptionDto>();

        var response = await pushClient.RevokeCurrentSubscriptionAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var subscription = await Db.QueryAsync(context =>
            context.PushSubscriptions.SingleAsync(x => x.Id == created.Id));
        subscription.RevokedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeCurrentSubscription_ForAnotherUserSubscription_ShouldNotRevoke()
    {
        var owner = await Users.RegisterClientAsync("push-revoke-owner-client");
        var anotherClient = await Users.RegisterClientAsync("push-revoke-another-client");
        var ownerPushClient = await Api.PushAsync(owner.Auth);
        var anotherPushClient = await Api.PushAsync(anotherClient.Auth);
        var endpoint = $"https://push.example.com/{Guid.NewGuid():D}";
        var registerResponse = await ownerPushClient.RegisterSubscriptionAsync(endpoint);
        var created = await registerResponse.ReadRequiredJsonAsync<PushSubscriptionDto>();

        var response = await anotherPushClient.RevokeCurrentSubscriptionAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var subscription = await Db.QueryAsync(context =>
            context.PushSubscriptions.SingleAsync(x => x.Id == created.Id));
        subscription.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task RevokeCurrentSubscription_WithoutCsrf_ShouldReturnBadRequest()
    {
        var client = await Users.RegisterClientAsync("push-revoke-csrf-client");
        var pushClient = await Api.PushAsync(client.Auth);

        var response = await pushClient.RevokeCurrentSubscriptionAsync(
            $"https://push.example.com/{Guid.NewGuid():D}",
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
