using System.Net;
using FitLead.Application.Messenger.Chats.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatListTests : MessengerTestBase
{
    public ChatListTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task TrainerChats_ShouldReturnOnlyOwnClientChats()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-list-trainer");
        var ownClient = await Users.RegisterClientAsync("chat-list-own-client");
        var otherTrainer = await Users.RegisterTrainerAsync("chat-list-other-trainer");
        var otherClient = await Users.RegisterClientAsync("chat-list-other-client");
        await CreateRelationshipAsync(trainer.Id, ownClient.Id);
        await CreateRelationshipAsync(otherTrainer.Id, otherClient.Id);
        var ownChat = await CreateChatAsync(trainer.Id, ownClient.Id);
        var otherChat = await CreateChatAsync(otherTrainer.Id, otherClient.Id);
        var orphanClient = await Users.RegisterClientAsync("chat-list-orphan-client");
        var orphanChat = await CreateChatAsync(trainer.Id, orphanClient.Id);
        var chatsClient = await Api.ChatsAsync(trainer.Auth);

        var response = await chatsClient.GetChatsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chats = await response.ReadRequiredJsonAsync<IReadOnlyList<ChatListItemDto>>();
        var chat = chats.Should().ContainSingle().Subject;
        chat.Id.Should().Be(ownChat.Id);
        chat.TrainerId.Should().Be(trainer.Id);
        chat.ClientId.Should().Be(ownClient.Id);
        chat.TrainerName.Should().Be("Test Trainer");
        chat.ClientName.Should().Be("Test Client");
        chats.Should().NotContain(x => x.Id == otherChat.Id);
        chats.Should().NotContain(x => x.Id == orphanChat.Id);
    }

    [Fact]
    public async Task ClientChats_ShouldReturnOnlyOwnTrainerChat()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-list-client-trainer");
        var client = await Users.RegisterClientAsync("chat-list-client");
        var otherTrainer = await Users.RegisterTrainerAsync("chat-list-client-other-trainer");
        var otherClient = await Users.RegisterClientAsync("chat-list-client-other-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        await CreateRelationshipAsync(otherTrainer.Id, otherClient.Id);
        var ownChat = await CreateChatAsync(trainer.Id, client.Id);
        var otherChat = await CreateChatAsync(otherTrainer.Id, otherClient.Id);
        var orphanTrainer = await Users.RegisterTrainerAsync("chat-list-client-orphan-trainer");
        var orphanChat = await CreateChatAsync(orphanTrainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(client.Auth);

        var response = await chatsClient.GetChatsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chats = await response.ReadRequiredJsonAsync<IReadOnlyList<ChatListItemDto>>();
        var chat = chats.Should().ContainSingle().Subject;
        chat.Id.Should().Be(ownChat.Id);
        chat.TrainerId.Should().Be(trainer.Id);
        chat.ClientId.Should().Be(client.Id);
        chats.Should().NotContain(x => x.Id == otherChat.Id);
        chats.Should().NotContain(x => x.Id == orphanChat.Id);
    }

}
