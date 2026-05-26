using System.Net;
using FitLead.Application.Clients.ProgressPhotos;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.ClientProgressPhotos;

public sealed class ProgressPhotoListDeleteTests(IntegrationTestFixture fixture)
    : ProgressPhotosTestBase(fixture)
{
    [Fact]
    public async Task ClientCanListOwnProgressPhotos()
    {
        var own = await CreateClientWithProgressPhotosAsync("progress-photo-list-own");
        var other = await CreateClientWithProgressPhotosAsync("progress-photo-list-other");
        var olderAsset = await CreateMediaAssetAsync(own.ClientId);
        var newerAsset = await CreateMediaAssetAsync(own.ClientId);
        var otherAsset = await CreateMediaAssetAsync(other.ClientId);
        var older = await CreateProgressPhotoAsync(
            own.ProgressPhotos,
            olderAsset.Id,
            new DateOnly(2026, 5, 20),
            label: "Front");
        var newer = await CreateProgressPhotoAsync(
            own.ProgressPhotos,
            newerAsset.Id,
            new DateOnly(2026, 5, 25),
            label: "Back");
        await CreateProgressPhotoAsync(
            other.ProgressPhotos,
            otherAsset.Id,
            new DateOnly(2026, 5, 24));

        var response = await own.ProgressPhotos.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var photos = await response.ReadRequiredJsonAsync<IReadOnlyList<ClientProgressPhotoDto>>();
        photos.Select(photo => photo.Id)
            .Should()
            .Equal(newer.Id, older.Id);
        photos.Should().OnlyContain(photo => photo.ClientId == own.ClientId);
    }

    [Fact]
    public async Task ClientCanDeleteOwnProgressPhoto()
    {
        var setup = await CreateClientWithProgressPhotosAsync("progress-photo-delete");
        var mediaAsset = await CreateMediaAssetAsync(setup.ClientId);
        var photo = await CreateProgressPhotoAsync(setup.ProgressPhotos, mediaAsset.Id);

        var response = await setup.ProgressPhotos.DeleteAsync(photo.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var photoCount = await Db.QueryAsync(context =>
            context.ClientProgressPhotos.CountAsync());
        var mediaAssetCount = await Db.QueryAsync(context =>
            context.MediaAssets.CountAsync());
        photoCount.Should().Be(0);
        mediaAssetCount.Should().Be(1);
    }
}
