using System.Net;
using System.Text.Json;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportOutboxTests : MessengerTestBase
{
    public VideoReportOutboxTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateVideoReport_ShouldCreateChatMessageCreatedOutboxMessage()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-outbox-trainer");
        var client = await Users.RegisterClientAsync("video-report-outbox-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var video = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var chatsClient = await Api.ChatsAsync(client.Auth);

        var response = await chatsClient.CreateVideoReportAsync(
            chat.Id,
            "Squat check",
            [video.Id]);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chatMessage = await response.ReadRequiredJsonAsync<ChatMessageDto>();

        var outboxMessage = await Db.QueryAsync(async context =>
        {
            var messages = await context.OutboxMessages
                .AsNoTracking()
                .Where(message => message.Type == OutboxEventTypes.Messenger.ChatMessageCreated)
                .ToListAsync();

            return messages.Single(message =>
                HasChatMessageCreatedPayload(
                    message.Payload,
                    chat.Id,
                    chatMessage.Id));
        });

        outboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);
        AssertChatMessageCreatedPayload(
            outboxMessage.Payload,
            chat.Id,
            chatMessage.Id);
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

    private static void AssertChatMessageCreatedPayload(
        string payload,
        Guid expectedChatId,
        Guid expectedMessageId)
    {
        using var document = JsonDocument.Parse(payload);
        document.RootElement.GetProperty("chatId").GetGuid().Should().Be(expectedChatId);
        document.RootElement.GetProperty("messageId").GetGuid().Should().Be(expectedMessageId);
    }
}
