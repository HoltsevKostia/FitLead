using System.Net;
using FitLead.Application.Messenger.VideoReports.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportDetailsTests : MessengerTestBase
{
    public VideoReportDetailsTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ParticipantGetVideoReport_ShouldReturnDetailsWithOrderedMedia()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-details-trainer");
        var client = await Users.RegisterClientAsync("video-report-details-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var image = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Image, "image/png", null);
        var video = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [image.Id, video.Id]);
        var chats = await Api.ChatsAsync(trainer.Auth);

        var response = await chats.GetVideoReportAsync(chat.Id, report.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await response.ReadRequiredJsonAsync<VideoReportDetailsDto>();
        details.Id.Should().Be(report.Id);
        details.ChatId.Should().Be(chat.Id);
        details.Media.Select(media => media.Id).Should().Equal(image.Id, video.Id);
        details.Media.Select(media => media.OrderInReport).Should().Equal(1, 2);
    }

    [Fact]
    public async Task GetVideoReport_WithMismatchedChat_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-mismatch-trainer");
        var firstClient = await Users.RegisterClientAsync("video-report-mismatch-client-one");
        var secondClient = await Users.RegisterClientAsync("video-report-mismatch-client-two");
        await CreateRelationshipAsync(trainer.Id, firstClient.Id);
        await CreateRelationshipAsync(trainer.Id, secondClient.Id);
        var firstChat = await CreateChatAsync(trainer.Id, firstClient.Id);
        var secondChat = await CreateChatAsync(trainer.Id, secondClient.Id);
        var media = await CreateMediaAssetAsync(firstClient.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(firstChat, firstClient.Id, trainer.Id, [media.Id]);
        var chats = await Api.ChatsAsync(trainer.Auth);

        var response = await chats.GetVideoReportAsync(secondChat.Id, report.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("video_report.not_found");
    }

    [Fact]
    public async Task UnrelatedUserGetVideoReport_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-owner-trainer");
        var client = await Users.RegisterClientAsync("video-report-owner-client");
        var unrelatedTrainer = await Users.RegisterTrainerAsync("video-report-unrelated-trainer");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [media.Id]);
        var chats = await Api.ChatsAsync(unrelatedTrainer.Auth);

        var response = await chats.GetVideoReportAsync(chat.Id, report.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("chat.not_found");
    }
}
