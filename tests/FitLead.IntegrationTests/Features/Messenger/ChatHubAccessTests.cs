using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatHubAccessTests : MessengerTestBase
{
    public ChatHubAccessTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Participant_ShouldJoinOwnChatGroup()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-hub-access-trainer");
        var client = await Users.RegisterClientAsync("chat-hub-access-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        await using var hub = await ChatHubTestClient.ConnectAsync(Fixture, trainer.Auth);

        var action = async () => await hub.JoinChatAsync(chat.Id);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NonParticipant_ShouldNotJoinChatGroup()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-hub-owner-trainer");
        var client = await Users.RegisterClientAsync("chat-hub-owner-client");
        var unrelatedTrainer = await Users.RegisterTrainerAsync("chat-hub-unrelated-trainer");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        await using var hub = await ChatHubTestClient.ConnectAsync(Fixture, unrelatedTrainer.Auth);

        var action = async () => await hub.JoinChatAsync(chat.Id);

        await action.Should().ThrowAsync<HubException>();
    }

    [Fact]
    public async Task ReconnectedConnection_ShouldRequireJoinChatAgain()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-hub-reconnect-trainer");
        var client = await Users.RegisterClientAsync("chat-hub-reconnect-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        await using var hub = await ChatHubTestClient.ConnectAsync(Fixture, trainer.Auth);
        await hub.JoinChatAsync(chat.Id);
        await hub.StopAsync();
        await hub.StartAsync();

        var delivery = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = hub.OnMessageCreated<object>(_ => delivery.TrySetResult());
        var chatsClient = await Api.ChatsAsync(client.Auth);

        await chatsClient.SendTextMessageAsync(chat.Id, "Після reconnect");

        var completedTask = await Task.WhenAny(
            delivery.Task,
            Task.Delay(TimeSpan.FromMilliseconds(300)));

        completedTask.Should().NotBe(delivery.Task);
    }
}
