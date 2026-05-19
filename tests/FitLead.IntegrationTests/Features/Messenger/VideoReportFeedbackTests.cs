using System.Net;
using FitLead.Application.Messenger.VideoReports.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportFeedbackTests : MessengerTestBase
{
    public VideoReportFeedbackTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task TrainerSubmitFeedback_ToOwnClientReport_ShouldReviewReport()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-feedback-trainer");
        var client = await Users.RegisterClientAsync("video-report-feedback-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [media.Id]);
        var chats = await Api.ChatsAsync(trainer.Auth);

        var response = await chats.SubmitVideoReportFeedbackAsync(
            chat.Id,
            report.Id,
            "  Keep your knees controlled.  ");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await response.ReadRequiredJsonAsync<VideoReportDetailsDto>();
        details.Status.Should().Be(VideoReportStatus.Reviewed.ToString());
        details.TrainerFeedbackText.Should().Be("Keep your knees controlled.");
        details.ReviewedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task ClientSubmitFeedback_ShouldReturnForbidden()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-feedback-role-trainer");
        var client = await Users.RegisterClientAsync("video-report-feedback-role-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [media.Id]);
        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.SubmitVideoReportFeedbackAsync(chat.Id, report.Id, "Review");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnrelatedTrainerSubmitFeedback_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-feedback-owner-trainer");
        var client = await Users.RegisterClientAsync("video-report-feedback-owner-client");
        var unrelatedTrainer = await Users.RegisterTrainerAsync("video-report-feedback-unrelated-trainer");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [media.Id]);
        var chats = await Api.ChatsAsync(unrelatedTrainer.Auth);

        var response = await chats.SubmitVideoReportFeedbackAsync(chat.Id, report.Id, "Review");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("chat.not_found");
    }

    [Fact]
    public async Task TrainerSubmitFeedback_SecondTime_ShouldReturnConflict()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-feedback-duplicate-trainer");
        var client = await Users.RegisterClientAsync("video-report-feedback-duplicate-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [media.Id]);
        var chats = await Api.ChatsAsync(trainer.Auth);
        await chats.SubmitVideoReportFeedbackAsync(chat.Id, report.Id, "First review");

        var response = await chats.SubmitVideoReportFeedbackAsync(chat.Id, report.Id, "Second review");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("video_report.review.already_reviewed");
    }
}
