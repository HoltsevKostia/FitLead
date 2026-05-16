using System.Net;
using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.MediaAssets;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class MediaAssetListTests : MediaAssetTestBase
{
    public MediaAssetListTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetMyAssets_ShouldReturnOnlyOwnActiveAssetsNewestFirst()
    {
        var owner = await Users.RegisterTrainerAsync("media-list-owner");
        var otherUser = await Users.RegisterTrainerAsync("media-list-other");
        var olderAsset = await CreateMediaAssetAsync(
            owner.Id,
            MediaAssetKind.Image,
            "image/png",
            durationSeconds: null,
            createdAtUtc: DateTime.UtcNow.AddMinutes(-5));
        var newerAsset = await CreateMediaAssetAsync(
            owner.Id,
            MediaAssetKind.Video,
            "video/mp4",
            createdAtUtc: DateTime.UtcNow);
        await CreateMediaAssetAsync(otherUser.Id, MediaAssetKind.Audio, "audio/mpeg");
        var mediaAssets = await Api.MediaAssetsAsync(owner.Auth);

        var response = await mediaAssets.GetMyAssetsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.ReadRequiredJsonAsync<IReadOnlyList<MediaAssetDto>>();
        items.Select(item => item.Id).Should().Equal(newerAsset.Id, olderAsset.Id);
    }
}
