using System.Net;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatRealtimeMessageTests : MessengerTestBase
{
    public ChatRealtimeMessageTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task JoinedParticipant_ShouldReceiveMessageCreatedAfterHttpSend()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-rt-trainer");
        var client = await Users.RegisterClientAsync("chat-rt-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        await using var hub = await ChatHubTestClient.ConnectAsync(Fixture, trainer.Auth);
        await hub.JoinChatAsync(chat.Id);
        var delivery = new TaskCompletionSource<ChatMessageDto>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = hub.OnMessageCreated<ChatMessageDto>(
            message => delivery.TrySetResult(message));
        var chatsClient = await Api.ChatsAsync(client.Auth);

        var response = await chatsClient.SendTextMessageAsync(chat.Id, "Готово");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var httpMessage = await response.ReadRequiredJsonAsync<ChatMessageDto>();
        var realtimeMessage = await delivery.Task.WaitAsync(TimeSpan.FromSeconds(2));
        realtimeMessage.Should().BeEquivalentTo(httpMessage);
        realtimeMessage.SenderName.Should().Be("Test Client");
    }
}
