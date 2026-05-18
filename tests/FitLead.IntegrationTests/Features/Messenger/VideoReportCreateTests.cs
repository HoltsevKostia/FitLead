using System.Net;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportCreateTests : MessengerTestBase
{
    public VideoReportCreateTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ClientCreateVideoReport_WithOwnedMedia_ShouldCreateReportAndChatMessage()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-trainer");
        var client = await Users.RegisterClientAsync("video-report-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var image = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Image, "image/png", null);
        var video = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.CreateVideoReportAsync(
            chat.Id,
            "Squat check",
            [image.Id, video.Id],
            "Please review");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var message = await response.ReadRequiredJsonAsync<ChatMessageDto>();
        message.Type.Should().Be(ChatMessageType.VideoReport.ToString());
        message.Text.Should().BeNull();
        message.VideoReport.Should().NotBeNull();
        message.VideoReport!.Title.Should().Be("Squat check");
        message.VideoReport.MediaCount.Should().Be(2);

        var persisted = await Db.QueryAsync(async context =>
        {
            var report = await context.VideoReports
                .Include(videoReport => videoReport.Media)
                .SingleAsync(videoReport => videoReport.Id == message.VideoReport.Id);
            var persistedMessage = await context.ChatMessages
                .SingleAsync(chatMessage => chatMessage.Id == message.Id);
            var updatedChat = await context.Chats
                .SingleAsync(candidate => candidate.Id == chat.Id);

            return new
            {
                Report = report,
                Message = persistedMessage,
                updatedChat.LastMessageAtUtc
            };
        });

        persisted.Report.Status.Should().Be(VideoReportStatus.Submitted);
        var orderedMedia = persisted.Report.Media
            .OrderBy(media => media.OrderInReport)
            .ToArray();
        orderedMedia.Select(media => media.MediaAssetId).Should().Equal(image.Id, video.Id);
        orderedMedia.Select(media => media.OrderInReport).Should().Equal(1, 2);
        persisted.Message.VideoReportId.Should().Be(persisted.Report.Id);
        persisted.LastMessageAtUtc.Should().Be(persisted.Message.CreatedAtUtc);
    }

    [Fact]
    public async Task TrainerCreateVideoReport_ShouldReturnForbidden()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-trainer-role");
        var client = await Users.RegisterClientAsync("video-report-client-role");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(trainer.Id, MediaAssetKind.Video, "video/mp4");
        var chats = await Api.ChatsAsync(trainer.Auth);

        var response = await chats.CreateVideoReportAsync(chat.Id, "Squat check", [media.Id]);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ClientCreateVideoReport_WithForeignMedia_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-owner-trainer");
        var client = await Users.RegisterClientAsync("video-report-owner-client");
        var otherClient = await Users.RegisterClientAsync("video-report-other-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var foreignMedia = await CreateMediaAssetAsync(otherClient.Id, MediaAssetKind.Video, "video/mp4");
        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.CreateVideoReportAsync(chat.Id, "Squat check", [foreignMedia.Id]);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("media_asset.not_found");
    }

    [Fact]
    public async Task ClientCreateVideoReport_InUnrelatedChat_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-chat-owner-trainer");
        var client = await Users.RegisterClientAsync("video-report-chat-owner-client");
        var unrelatedClient = await Users.RegisterClientAsync("video-report-unrelated-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(unrelatedClient.Id, MediaAssetKind.Video, "video/mp4");
        var chats = await Api.ChatsAsync(unrelatedClient.Auth);

        var response = await chats.CreateVideoReportAsync(chat.Id, "Squat check", [media.Id]);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("chat.not_found");
    }

    [Fact]
    public async Task ClientCreateVideoReport_WithDuplicateMedia_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-duplicate-trainer");
        var client = await Users.RegisterClientAsync("video-report-duplicate-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.CreateVideoReportAsync(chat.Id, "Squat check", [media.Id, media.Id]);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("video_report.create.duplicate_media_assets");
    }

    [Fact]
    public async Task ClientCreateVideoReport_WithAudioMedia_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-audio-trainer");
        var client = await Users.RegisterClientAsync("video-report-audio-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var audio = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Audio, "audio/mpeg");
        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.CreateVideoReportAsync(chat.Id, "Squat check", [audio.Id]);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("media_asset.kind_not_allowed_for_video_report");
    }
}
