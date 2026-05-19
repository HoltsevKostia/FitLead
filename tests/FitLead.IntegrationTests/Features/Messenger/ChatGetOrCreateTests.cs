using System.Net;
using FitLead.Application.Messenger.Chats.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatGetOrCreateTests : MessengerTestBase
{
    public ChatGetOrCreateTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task TrainerGetOrCreateWithOwnClient_ShouldCreateChat()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-trainer-create");
        var client = await Users.RegisterClientAsync("chat-client-create");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chats = await Api.ChatsAsync(trainer.Auth);

        var response = await chats.GetOrCreateWithClientAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chat = await response.ReadRequiredJsonAsync<ChatDto>();
        chat.Id.Should().NotBeEmpty();
        chat.TrainerId.Should().Be(trainer.Id);
        chat.ClientId.Should().Be(client.Id);
        chat.LastMessageAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task TrainerGetOrCreateSecondCall_ShouldReturnSameChat()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-trainer-idempotent");
        var client = await Users.RegisterClientAsync("chat-client-idempotent");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chats = await Api.ChatsAsync(trainer.Auth);

        var firstResponse = await chats.GetOrCreateWithClientAsync(client.Id);
        var secondResponse = await chats.GetOrCreateWithClientAsync(client.Id);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var first = await firstResponse.ReadRequiredJsonAsync<ChatDto>();
        var second = await secondResponse.ReadRequiredJsonAsync<ChatDto>();
        second.Id.Should().Be(first.Id);

        var chatCount = await Db.QueryAsync(context =>
            context.Chats.CountAsync(x =>
                x.TrainerId == trainer.Id &&
                x.ClientId == client.Id));
        chatCount.Should().Be(1);
    }

    [Fact]
    public async Task TrainerGetOrCreateWithUnrelatedClient_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-trainer-unrelated");
        var client = await Users.RegisterClientAsync("chat-client-unrelated");
        var chats = await Api.ChatsAsync(trainer.Auth);

        var response = await chats.GetOrCreateWithClientAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("chat.not_found");
    }

    [Fact]
    public async Task ClientGetOrCreateWithOwnTrainer_ShouldCreateChat()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-trainer-client-open");
        var client = await Users.RegisterClientAsync("chat-client-open");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.GetOrCreateWithTrainerAsync(trainer.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chat = await response.ReadRequiredJsonAsync<ChatDto>();
        chat.TrainerId.Should().Be(trainer.Id);
        chat.ClientId.Should().Be(client.Id);
    }

}
