using System.Net;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Domain.Messenger.ChatMessages;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatMessageSendTests : MessengerTestBase
{
    public ChatMessageSendTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task TrainerSendTextMessage_ToOwnClientChat_ShouldCreateMessageAndUpdateChat()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-send-trainer");
        var client = await Users.RegisterClientAsync("chat-send-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(trainer.Auth);

        var response = await chatsClient.SendTextMessageAsync(chat.Id, "  Привіт  ");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var message = await response.ReadRequiredJsonAsync<ChatMessageDto>();
        message.Id.Should().NotBeEmpty();
        message.ChatId.Should().Be(chat.Id);
        message.SenderId.Should().Be(trainer.Id);
        message.Type.Should().Be(ChatMessageType.Text.ToString());
        message.Text.Should().Be("Привіт");

        var persisted = await Db.QueryAsync(async context =>
        {
            var updatedChat = await context.Chats
                .AsNoTracking()
                .SingleAsync(x => x.Id == chat.Id);
            var persistedMessage = await context.ChatMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == message.Id);

            return new
            {
                updatedChat.LastMessageAtUtc,
                persistedMessage.CreatedAtUtc
            };
        });
        persisted.LastMessageAtUtc.Should().Be(persisted.CreatedAtUtc);
    }

    [Fact]
    public async Task ClientSendTextMessage_ToOwnTrainerChat_ShouldCreateMessage()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-send-client-trainer");
        var client = await Users.RegisterClientAsync("chat-send-client-user");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(client.Auth);

        var response = await chatsClient.SendTextMessageAsync(chat.Id, "Готово");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var message = await response.ReadRequiredJsonAsync<ChatMessageDto>();
        message.ChatId.Should().Be(chat.Id);
        message.SenderId.Should().Be(client.Id);
        message.Text.Should().Be("Готово");
    }

    [Fact]
    public async Task UnrelatedUserSendTextMessage_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-send-owner-trainer");
        var client = await Users.RegisterClientAsync("chat-send-owner-client");
        var unrelatedTrainer = await Users.RegisterTrainerAsync("chat-send-unrelated-trainer");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(unrelatedTrainer.Auth);

        var response = await chatsClient.SendTextMessageAsync(chat.Id, "Привіт");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("chat.not_found");
    }

}
