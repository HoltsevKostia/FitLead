using System.Net;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.ChatMessages;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportHistoryTests : MessengerTestBase
{
    public VideoReportHistoryTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetMessages_WithVideoReportMessage_ShouldReturnPreview()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-history-trainer");
        var client = await Users.RegisterClientAsync("video-report-history-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var image = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Image, "image/png", null);
        var video = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [image.Id, video.Id]);
        var message = ChatMessage.CreateVideoReport(chat, report, client.Id, DateTime.UtcNow).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.ChatMessages.AddAsync(message);
            await context.SaveChangesAsync();
        });

        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.GetMessagesAsync(chat.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ReadRequiredJsonAsync<ChatMessageHistoryDto>();
        history.Items.Should().ContainSingle();
        var preview = history.Items[0].VideoReport;
        preview.Should().NotBeNull();
        preview!.Id.Should().Be(report.Id);
        preview.Title.Should().Be(report.Title);
        preview.MediaCount.Should().Be(2);
    }
}
