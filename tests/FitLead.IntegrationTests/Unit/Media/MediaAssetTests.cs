using FitLead.Domain.Media.MediaAssets;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Media;

public sealed class MediaAssetTests
{
    [Fact]
    public void Create_WithValidVideoMetadata_ShouldCreateActiveAsset()
    {
        var ownerUserId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var result = MediaAsset.Create(
            ownerUserId,
            MediaStorageProvider.Uploadcare,
            "  uploadcare-object  ",
            "  https://ucarecdn.example/uploadcare-object/  ",
            "  video.mp4  ",
            "  video/mp4  ",
            1024,
            MediaAssetKind.Video,
            42,
            createdAtUtc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.OwnerUserId.Should().Be(ownerUserId);
        result.Value.StorageProvider.Should().Be(MediaStorageProvider.Uploadcare);
        result.Value.StorageObjectId.Should().Be("uploadcare-object");
        result.Value.DeliveryUrl.Should().Be("https://ucarecdn.example/uploadcare-object/");
        result.Value.FileName.Should().Be("video.mp4");
        result.Value.ContentType.Should().Be("video/mp4");
        result.Value.SizeBytes.Should().Be(1024);
        result.Value.Kind.Should().Be(MediaAssetKind.Video);
        result.Value.DurationSeconds.Should().Be(42);
        result.Value.CreatedAtUtc.Should().Be(createdAtUtc);
        result.Value.Status.Should().Be(MediaAssetStatus.Active);
    }

    [Fact]
    public void Create_WithEmptyOwnerUserId_ShouldReturnValidationError()
    {
        var result = Create(ownerUserId: Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.owner_user_id_required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyStorageObjectId_ShouldReturnValidationError(string storageObjectId)
    {
        var result = Create(storageObjectId: storageObjectId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.storage_object_id_required");
    }

    [Fact]
    public void Create_WithExternalHttpDeliveryUrl_ShouldReturnValidationError()
    {
        var result = Create(deliveryUrl: "http://ucarecdn.example/uploadcare-object/");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.delivery_url_invalid_scheme");
    }

    [Fact]
    public void Create_WithLocalDevHttpDeliveryUrl_ShouldCreateAsset()
    {
        var result = Create(
            storageProvider: MediaStorageProvider.LocalDev,
            deliveryUrl: "http://localhost:5178/media/uploadcare-object/");

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("video")]
    [InlineData("/mp4")]
    [InlineData("video/")]
    public void Create_WithInvalidMimeShape_ShouldReturnValidationError(string contentType)
    {
        var result = Create(contentType: contentType);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.content_type_invalid");
    }

    [Theory]
    [InlineData(MediaAssetKind.Image, "video/mp4")]
    [InlineData(MediaAssetKind.Video, "audio/mpeg")]
    [InlineData(MediaAssetKind.Audio, "image/png")]
    public void Create_WithContentTypeKindMismatch_ShouldReturnValidationError(
        MediaAssetKind kind,
        string contentType)
    {
        var result = Create(
            kind: kind,
            contentType: contentType,
            durationSeconds: kind == MediaAssetKind.Image ? null : 12);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.content_type_kind_mismatch");
    }

    [Fact]
    public void Create_WithImageDuration_ShouldReturnValidationError()
    {
        var result = Create(
            kind: MediaAssetKind.Image,
            contentType: "image/png",
            durationSeconds: 12);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.image_duration_not_allowed");
    }

    [Fact]
    public void Create_WithNonPositiveDuration_ShouldReturnValidationError()
    {
        var result = Create(durationSeconds: 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.duration_seconds_invalid");
    }

    [Fact]
    public void Create_WithNonPositiveSize_ShouldReturnValidationError()
    {
        var result = Create(sizeBytes: 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.size_bytes_invalid");
    }

    [Fact]
    public void Create_WithDefaultCreatedAt_ShouldReturnValidationError()
    {
        var result = Create(createdAtUtc: default(DateTime));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_asset.create.created_at_required");
    }

    private static FitLead.Common.Results.Result<MediaAsset> Create(
        Guid? ownerUserId = null,
        MediaStorageProvider storageProvider = MediaStorageProvider.Uploadcare,
        string storageObjectId = "uploadcare-object",
        string deliveryUrl = "https://ucarecdn.example/uploadcare-object/",
        string? fileName = "video.mp4",
        string contentType = "video/mp4",
        long sizeBytes = 1024,
        MediaAssetKind kind = MediaAssetKind.Video,
        int? durationSeconds = 12,
        DateTime? createdAtUtc = null)
    {
        return MediaAsset.Create(
            ownerUserId ?? Guid.NewGuid(),
            storageProvider,
            storageObjectId,
            deliveryUrl,
            fileName,
            contentType,
            sizeBytes,
            kind,
            durationSeconds,
            createdAtUtc ?? DateTime.UtcNow);
    }
}
