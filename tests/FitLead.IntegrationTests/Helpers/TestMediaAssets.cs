using FitLead.Domain.Media.MediaAssets;

namespace FitLead.IntegrationTests.Helpers;

public sealed class TestMediaAssets(TestDb db)
{
    public async Task<MediaAsset> CreateAsync(
        Guid ownerUserId,
        MediaAssetKind kind = MediaAssetKind.Image,
        string? contentType = null,
        int? durationSeconds = null)
    {
        contentType ??= kind switch
        {
            MediaAssetKind.Image => "image/png",
            MediaAssetKind.Video => "video/mp4",
            MediaAssetKind.Audio => "audio/mpeg",
            _ => "application/octet-stream"
        };

        var mediaAsset = MediaAsset.Create(
            ownerUserId,
            MediaStorageProvider.Uploadcare,
            Guid.NewGuid().ToString(),
            $"https://ucarecdn.example/{Guid.NewGuid():D}/",
            "exercise-media.bin",
            contentType,
            1024,
            kind,
            kind == MediaAssetKind.Image ? null : durationSeconds ?? 12,
            DateTime.UtcNow).Value;

        await db.ExecuteAsync(async context =>
        {
            await context.MediaAssets.AddAsync(mediaAsset);
            await context.SaveChangesAsync();
        });

        return mediaAsset;
    }
}
