using FitLead.Application.Media.MediaAssets.Access;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.MediaAssets;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class MediaAssetAccessTests : MediaAssetTestBase
{
    public MediaAssetAccessTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetOwnedOrNotFound_WithOwnedAsset_ShouldReturnAsset()
    {
        var owner = await Users.RegisterTrainerAsync("media-access-owner");
        var mediaAsset = await CreateMediaAssetAsync(owner.Id, MediaAssetKind.Video, "video/mp4");

        var result = await LoadWithAsync(loader =>
            loader.GetOwnedOrNotFoundAsync(owner.Id, mediaAsset.Id, CancellationToken.None));

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(mediaAsset.Id);
    }

    [Fact]
    public async Task GetOwnedOrNotFound_WithForeignAsset_ShouldReturnNotFound()
    {
        var owner = await Users.RegisterTrainerAsync("media-access-owner-foreign");
        var otherUser = await Users.RegisterTrainerAsync("media-access-other");
        var mediaAsset = await CreateMediaAssetAsync(otherUser.Id, MediaAssetKind.Video, "video/mp4");

        var result = await LoadWithAsync(loader =>
            loader.GetOwnedOrNotFoundAsync(owner.Id, mediaAsset.Id, CancellationToken.None));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.not_found");
    }

    [Fact]
    public async Task GetOwnedAllowedForVideoReport_WithOwnedImageAndVideo_ShouldReturnAssets()
    {
        var owner = await Users.RegisterTrainerAsync("media-report-owner");
        var image = await CreateMediaAssetAsync(
            owner.Id,
            MediaAssetKind.Image,
            "image/png",
            durationSeconds: null);
        var video = await CreateMediaAssetAsync(owner.Id, MediaAssetKind.Video, "video/mp4");

        var result = await LoadWithAsync(loader =>
            loader.GetOwnedAllowedForVideoReportOrNotFoundAsync(
                owner.Id,
                [image.Id, video.Id],
                CancellationToken.None));

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(mediaAsset => mediaAsset.Id)
            .Should()
            .BeEquivalentTo([image.Id, video.Id]);
    }

    [Fact]
    public async Task GetOwnedAllowedForVideoReport_WithForeignAsset_ShouldReturnNotFound()
    {
        var owner = await Users.RegisterTrainerAsync("media-report-owner-foreign");
        var otherUser = await Users.RegisterTrainerAsync("media-report-other");
        var ownedAsset = await CreateMediaAssetAsync(owner.Id, MediaAssetKind.Video, "video/mp4");
        var foreignAsset = await CreateMediaAssetAsync(
            otherUser.Id,
            MediaAssetKind.Image,
            "image/png",
            durationSeconds: null);

        var result = await LoadWithAsync(loader =>
            loader.GetOwnedAllowedForVideoReportOrNotFoundAsync(
                owner.Id,
                [ownedAsset.Id, foreignAsset.Id],
                CancellationToken.None));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.not_found");
    }

    [Fact]
    public async Task GetOwnedAllowedForVideoReport_WithAudioAsset_ShouldReturnValidationError()
    {
        var owner = await Users.RegisterTrainerAsync("media-report-audio-owner");
        var audio = await CreateMediaAssetAsync(owner.Id, MediaAssetKind.Audio, "audio/mpeg");

        var result = await LoadWithAsync(loader =>
            loader.GetOwnedAllowedForVideoReportOrNotFoundAsync(
                owner.Id,
                [audio.Id],
                CancellationToken.None));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.kind_not_allowed_for_video_report");
    }

    private async Task<TResult> LoadWithAsync<TResult>(
        Func<IMediaAssetLoader, Task<TResult>> action)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var loader = scope.ServiceProvider.GetRequiredService<IMediaAssetLoader>();
        return await action(loader);
    }
}
