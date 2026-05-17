using FitLead.Domain.Media.MediaAssets;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;

namespace FitLead.IntegrationTests.Features.MediaAssets;

public abstract class MediaAssetTestBase : IntegrationTestBase
{
    protected readonly TestDb Db;
    protected readonly TestUsers Users;
    protected readonly TestApiClients Api;

    protected MediaAssetTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Users = new TestUsers(fixture, Db);
        Api = new TestApiClients(fixture);
    }

    protected async Task<MediaAsset> CreateMediaAssetAsync(
        Guid ownerUserId,
        MediaAssetKind kind,
        string contentType,
        int? durationSeconds = 12,
        DateTime? createdAtUtc = null)
    {
        var mediaAsset = MediaAsset.Create(
            ownerUserId,
            MediaStorageProvider.Uploadcare,
            Guid.NewGuid().ToString(),
            $"https://ucarecdn.example/{Guid.NewGuid():D}/",
            "file.bin",
            contentType,
            1024,
            kind,
            durationSeconds,
            createdAtUtc ?? DateTime.UtcNow).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.MediaAssets.AddAsync(mediaAsset);
            await context.SaveChangesAsync();
        });

        return mediaAsset;
    }
}
