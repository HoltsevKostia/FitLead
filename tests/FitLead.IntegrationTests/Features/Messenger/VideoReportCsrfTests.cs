using System.Net;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class VideoReportCsrfTests : MessengerTestBase
{
    public VideoReportCsrfTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateVideoReport_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await Users.RegisterTrainerAsync("video-report-csrf-trainer");
        var client = await Users.RegisterClientAsync("video-report-csrf-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var media = await CreateMediaAssetAsync(client.Id, MediaAssetKind.Video, "video/mp4");
        var chats = await Api.ChatsAsync(client.Auth);

        var response = await chats.CreateVideoReportAsync(
            chat.Id,
            "Squat check",
            [media.Id],
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
