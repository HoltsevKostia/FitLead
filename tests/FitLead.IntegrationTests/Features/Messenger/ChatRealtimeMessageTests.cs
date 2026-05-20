using System.Net;
using System.Text.Json;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        var outboxMessageId = await Db.QueryAsync(async context =>
        {
            var messages = await context.OutboxMessages
                .AsNoTracking()
                .ToListAsync();

            return messages.Single(message =>
                HasChatMessageCreatedPayload(
                    message.Payload,
                    chat.Id,
                    httpMessage.Id)).Id;
        });
        await using (var scope = Fixture.Factory.Services.CreateAsyncScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
            await processor.ProcessAsync(outboxMessageId, CancellationToken.None);
        }

        var realtimeMessage = await delivery.Task.WaitAsync(TimeSpan.FromSeconds(5));
        realtimeMessage.Should().BeEquivalentTo(
            httpMessage,
            options => options.Excluding(message => message.CreatedAtUtc));
        realtimeMessage.CreatedAtUtc.Should().BeCloseTo(
            httpMessage.CreatedAtUtc,
            TimeSpan.FromMilliseconds(1));
        realtimeMessage.SenderName.Should().Be("Test Client");
    }

    private static bool HasChatMessageCreatedPayload(
        string payload,
        Guid expectedChatId,
        Guid expectedMessageId)
    {
        using var document = JsonDocument.Parse(payload);

        return document.RootElement.GetProperty("chatId").GetGuid() == expectedChatId &&
               document.RootElement.GetProperty("messageId").GetGuid() == expectedMessageId;
    }
}
