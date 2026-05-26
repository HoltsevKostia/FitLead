using System.Net;
using FitLead.Application.Clients.ProgressPhotos;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.ClientProgressPhotos;

public sealed class ProgressPhotoCreateTests(IntegrationTestFixture fixture)
    : ProgressPhotosTestBase(fixture)
{
    [Fact]
    public async Task ClientCanCreateProgressPhotoWithOwnImageMediaAsset()
    {
        var setup = await CreateClientWithProgressPhotosAsync("progress-photo-create");
        var mediaAsset = await CreateMediaAssetAsync(setup.ClientId);
        var takenAt = new DateOnly(2026, 5, 25);

        var response = await setup.ProgressPhotos.CreateAsync(
            mediaAsset.Id,
            takenAt,
            label: "Side",
            note: "First progress check");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.ReadRequiredJsonAsync<ClientProgressPhotoDto>();
        dto.ClientId.Should().Be(setup.ClientId);
        dto.MediaAssetId.Should().Be(mediaAsset.Id);
        dto.TakenAt.Should().Be(takenAt);
        dto.Label.Should().Be("Side");
        dto.Note.Should().Be("First progress check");
        dto.MediaAsset.Id.Should().Be(mediaAsset.Id);
        dto.MediaAsset.Kind.Should().Be(nameof(MediaAssetKind.Image));

        var persisted = await Db.QueryAsync(context =>
            context.ClientProgressPhotos.SingleAsync());
        persisted.Id.Should().Be(dto.Id);
        persisted.MediaAssetId.Should().Be(mediaAsset.Id);
    }
}
