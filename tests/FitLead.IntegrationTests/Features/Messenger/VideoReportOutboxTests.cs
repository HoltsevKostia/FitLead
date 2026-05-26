using System.Net;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.ChatMessages.Outbox;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Application.Messenger.VideoReports.Outbox;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

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

        var outboxMessage = await Outbox.GetSingleAsync<ChatMessageCreatedOutboxPayload>(
            OutboxEventTypes.Messenger.ChatMessageCreated,
            payload => payload.ChatId == chat.Id &&
                       payload.MessageId == chatMessage.Id);

        outboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);

        var submittedOutboxMessage = await Outbox.GetSingleAsync<VideoReportSubmittedOutboxPayload>(
            OutboxEventTypes.Messenger.VideoReportSubmitted,
            payload => payload.ChatId == chat.Id &&
                       payload.ReportId == chatMessage.VideoReport!.Id &&
                       payload.ClientId == client.Id &&
                       payload.TrainerId == trainer.Id &&
                       payload.Title == "Squat check");

        submittedOutboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);
    }
}
