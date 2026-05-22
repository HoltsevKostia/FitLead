using System.Net;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.ChatMessages.Outbox;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatMessageOutboxTests : MessengerTestBase
{
    public ChatMessageOutboxTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task SendTextMessage_ShouldCreateChatMessageCreatedOutboxMessage()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-outbox-trainer");
        var client = await Users.RegisterClientAsync("chat-outbox-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(client.Auth);

        var response = await chatsClient.SendTextMessageAsync(chat.Id, "Hello");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chatMessage = await response.ReadRequiredJsonAsync<ChatMessageDto>();

        var outboxMessage = await Outbox.GetSingleAsync<ChatMessageCreatedOutboxPayload>(
            OutboxEventTypes.Messenger.ChatMessageCreated,
            payload => payload.ChatId == chat.Id &&
                       payload.MessageId == chatMessage.Id);

        outboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);
    }
}
