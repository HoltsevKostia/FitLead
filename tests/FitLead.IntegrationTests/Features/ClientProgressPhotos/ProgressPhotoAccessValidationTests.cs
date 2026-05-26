using System.Net;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.ClientProgressPhotos;

public sealed class ProgressPhotoAccessValidationTests(IntegrationTestFixture fixture)
    : ProgressPhotosTestBase(fixture)
{
    [Fact]
    public async Task Create_WithNonImageMedia_ShouldReturnValidationError()
    {
        var setup = await CreateClientWithProgressPhotosAsync("progress-photo-non-image");
        var mediaAsset = await CreateMediaAssetAsync(
            setup.ClientId,
            MediaAssetKind.Video,
            contentType: "video/mp4",
            durationSeconds: 12);

        var response = await setup.ProgressPhotos.CreateAsync(mediaAsset.Id);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("media_asset.kind_not_allowed_for_progress_photo");
    }

    [Fact]
    public async Task Create_WithMediaOwnedByAnotherUser_ShouldReturnNotFound()
    {
        var owner = await CreateClientWithProgressPhotosAsync("progress-photo-media-owner");
        var other = await CreateClientWithProgressPhotosAsync("progress-photo-media-other");
        var mediaAsset = await CreateMediaAssetAsync(owner.ClientId);

        var response = await other.ProgressPhotos.CreateAsync(mediaAsset.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("media_asset.not_found");
    }

    [Fact]
    public async Task TrainerCannotMutateProgressPhotosEndpoint()
    {
        var trainer = await Users.RegisterTrainerAsync("progress-photo-trainer");
        var progressPhotos = await Api.ProgressPhotosAsync(trainer.Auth);
        var mediaAsset = await CreateMediaAssetAsync(trainer.Id);

        var response = await progressPhotos.CreateAsync(mediaAsset.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ClientCannotDeleteAnotherClientProgressPhoto()
    {
        var owner = await CreateClientWithProgressPhotosAsync("progress-photo-owner-delete");
        var other = await CreateClientWithProgressPhotosAsync("progress-photo-other-delete");
        var mediaAsset = await CreateMediaAssetAsync(owner.ClientId);
        var photo = await CreateProgressPhotoAsync(owner.ProgressPhotos, mediaAsset.Id);

        var response = await other.ProgressPhotos.DeleteAsync(photo.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var count = await Db.QueryAsync(context =>
            context.ClientProgressPhotos.CountAsync());
        count.Should().Be(1);
    }
}
