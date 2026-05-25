using System.Net;
using FitLead.Application.Clients.ProgressPhotos;
using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.ClientProgressPhotos;

public abstract class ProgressPhotosTestBase : IntegrationTestBase
{
    protected readonly TestDb Db;
    protected readonly TestUsers Users;
    protected readonly TestApiClients Api;

    protected ProgressPhotosTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Users = new TestUsers(fixture, Db);
        Api = new TestApiClients(fixture);
    }

    protected async Task<ClientProgressPhotoSetup> CreateClientWithProgressPhotosAsync(
        string prefix)
    {
        var client = await Users.RegisterClientAsync(prefix);
        var progressPhotos = await Api.ProgressPhotosAsync(client.Auth);

        return new ClientProgressPhotoSetup(client.Id, progressPhotos);
    }

    protected async Task<MediaAsset> CreateMediaAssetAsync(
        Guid ownerUserId,
        MediaAssetKind kind = MediaAssetKind.Image,
        string contentType = "image/jpeg",
        int? durationSeconds = null)
    {
        var mediaAsset = MediaAsset.Create(
            ownerUserId,
            MediaStorageProvider.Uploadcare,
            Guid.NewGuid().ToString(),
            $"https://ucarecdn.example/{Guid.NewGuid():D}/",
            "progress.jpg",
            contentType,
            1024,
            kind,
            durationSeconds,
            DateTime.UtcNow).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.MediaAssets.AddAsync(mediaAsset);
            await context.SaveChangesAsync();
        });

        return mediaAsset;
    }

    protected static async Task<ClientProgressPhotoDto> CreateProgressPhotoAsync(
        ProgressPhotosTestClient progressPhotos,
        Guid mediaAssetId,
        DateOnly? takenAt = null,
        string? label = "Front")
    {
        var response = await progressPhotos.CreateAsync(
            mediaAssetId,
            takenAt ?? new DateOnly(2026, 5, 25),
            label);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return await response.ReadRequiredJsonAsync<ClientProgressPhotoDto>();
    }

    protected sealed record ClientProgressPhotoSetup(
        Guid ClientId,
        ProgressPhotosTestClient ProgressPhotos);
}
