using System.Net;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportFeedbackCsrfTests : MessengerTestBase
{
    public VideoReportFeedbackCsrfTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task SubmitFeedback_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-feedback-csrf-trainer");
        var client = await Users.RegisterClientAsync("video-report-feedback-csrf-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var report = await CreateVideoReportAsync(chat, client.Id, trainer.Id, [media.Id]);
        var chats = await Api.ChatsAsync(trainer.Auth);

        var response = await chats.SubmitVideoReportFeedbackAsync(
            chat.Id,
            report.Id,
            "Review",
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
