using System.Net;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.VideoReports.Outbox;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportFeedbackOutboxTests : MessengerTestBase
{
    public VideoReportFeedbackOutboxTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task SubmitFeedback_ShouldCreateVideoReportReviewedOutboxMessage()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-feedback-outbox-trainer");
        var client = await Users.RegisterClientAsync("video-report-feedback-outbox-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var video = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(
            chat,
            client.Id,
            trainer.Id,
            [video.Id],
            "Squat feedback");
        var chatsClient = await Api.ChatsAsync(trainer.Auth);

        var response = await chatsClient.SubmitVideoReportFeedbackAsync(
            chat.Id,
            report.Id,
            "Looks good.");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var outboxMessage = await Outbox.GetSingleAsync<VideoReportReviewedOutboxPayload>(
            OutboxEventTypes.Messenger.VideoReportReviewed,
            payload => payload.ChatId == chat.Id &&
                       payload.ReportId == report.Id &&
                       payload.ClientId == client.Id &&
                       payload.TrainerId == trainer.Id &&
                       payload.Title == "Squat feedback");

        outboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);
    }
}
