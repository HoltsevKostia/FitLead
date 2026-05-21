using System.Net;
using System.Text.Json;
using FitLead.Application.Common.Outbox;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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

        var outboxMessage = await Db.QueryAsync(async context =>
        {
            var messages = await context.OutboxMessages
                .AsNoTracking()
                .Where(message => message.Type == OutboxEventTypes.Messenger.VideoReportReviewed)
                .ToListAsync();

            return messages.Single(message =>
                HasVideoReportReviewedPayload(
                    message.Payload,
                    chat.Id,
                    report.Id,
                    client.Id,
                    trainer.Id,
                    "Squat feedback"));
        });

        outboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);
    }

    private static bool HasVideoReportReviewedPayload(
        string payload,
        Guid expectedChatId,
        Guid expectedReportId,
        Guid expectedClientId,
        Guid expectedTrainerId,
        string expectedTitle)
    {
        using var document = JsonDocument.Parse(payload);

        return document.RootElement.GetProperty("chatId").GetGuid() == expectedChatId &&
               document.RootElement.GetProperty("reportId").GetGuid() == expectedReportId &&
               document.RootElement.GetProperty("clientId").GetGuid() == expectedClientId &&
               document.RootElement.GetProperty("trainerId").GetGuid() == expectedTrainerId &&
               document.RootElement.GetProperty("title").GetString() == expectedTitle;
    }
}
