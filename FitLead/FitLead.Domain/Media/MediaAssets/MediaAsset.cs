using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Media.MediaAssets
{
    public sealed class MediaAsset : AggregateRoot<Guid>
    {
        public const int MaxStorageObjectIdLength = 500;
        public const int MaxDeliveryUrlLength = 2048;
        public const int MaxFileNameLength = 500;
        public const int MaxContentTypeLength = 200;

        public Guid OwnerUserId { get; private set; }
        public MediaStorageProvider StorageProvider { get; private set; }
        public string StorageObjectId { get; private set; } = string.Empty;
        public string DeliveryUrl { get; private set; } = string.Empty;
        public string? FileName { get; private set; }
        public string ContentType { get; private set; } = string.Empty;
        public long SizeBytes { get; private set; }
        public MediaAssetKind Kind { get; private set; }
        public int? DurationSeconds { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public MediaAssetStatus Status { get; private set; }

        private MediaAsset()
        {
        }

        private MediaAsset(
            Guid id,
            Guid ownerUserId,
            MediaStorageProvider storageProvider,
            string storageObjectId,
            string deliveryUrl,
            string? fileName,
            string contentType,
            long sizeBytes,
            MediaAssetKind kind,
            int? durationSeconds,
            DateTime createdAtUtc)
        {
            Id = id;
            OwnerUserId = ownerUserId;
            StorageProvider = storageProvider;
            StorageObjectId = storageObjectId;
            DeliveryUrl = deliveryUrl;
            FileName = fileName;
            ContentType = contentType;
            SizeBytes = sizeBytes;
            Kind = kind;
            DurationSeconds = durationSeconds;
            CreatedAtUtc = createdAtUtc;
            Status = MediaAssetStatus.Active;
        }

        public static Result<MediaAsset> Create(
            Guid ownerUserId,
            MediaStorageProvider storageProvider,
            string storageObjectId,
            string deliveryUrl,
            string? fileName,
            string contentType,
            long sizeBytes,
            MediaAssetKind kind,
            int? durationSeconds,
            DateTime createdAtUtc)
        {
            if (ownerUserId == Guid.Empty)
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.owner_user_id_required", "OwnerUserId is required"));
            }

            if (!Enum.IsDefined(storageProvider))
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.storage_provider_invalid", "StorageProvider is invalid"));
            }

            if (string.IsNullOrWhiteSpace(storageObjectId))
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.storage_object_id_required", "StorageObjectId is required"));
            }

            var trimmedStorageObjectId = storageObjectId.Trim();
            if (trimmedStorageObjectId.Length > MaxStorageObjectIdLength)
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.storage_object_id_too_long", $"StorageObjectId cannot exceed {MaxStorageObjectIdLength} characters"));
            }

            var deliveryUrlValidation = ValidateDeliveryUrl(storageProvider, deliveryUrl);
            if (deliveryUrlValidation.IsFailure)
            {
                return Result<MediaAsset>.Failure(deliveryUrlValidation.Error);
            }

            var normalizedFileNameResult = NormalizeOptionalFileName(fileName);
            if (normalizedFileNameResult.IsFailure)
            {
                return Result<MediaAsset>.Failure(normalizedFileNameResult.Error);
            }

            if (string.IsNullOrWhiteSpace(contentType))
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.content_type_required", "ContentType is required"));
            }

            var trimmedContentType = contentType.Trim();
            if (trimmedContentType.Length > MaxContentTypeLength)
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.content_type_too_long", $"ContentType cannot exceed {MaxContentTypeLength} characters"));
            }

            if (!HasBasicMimeShape(trimmedContentType))
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.content_type_invalid", "ContentType must be a valid MIME type"));
            }

            if (sizeBytes <= 0)
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.size_bytes_invalid", "SizeBytes must be greater than zero"));
            }

            if (!Enum.IsDefined(kind))
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.kind_invalid", "Kind is invalid"));
            }

            if (!MatchesKind(trimmedContentType, kind))
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.content_type_kind_mismatch", "ContentType does not match Kind"));
            }

            if (kind == MediaAssetKind.Image && durationSeconds.HasValue)
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.image_duration_not_allowed", "Image assets cannot have DurationSeconds"));
            }

            if (durationSeconds.HasValue && durationSeconds.Value <= 0)
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.duration_seconds_invalid", "DurationSeconds must be greater than zero when provided"));
            }

            if (createdAtUtc == default)
            {
                return Result<MediaAsset>.Failure(
                    Error.Validation("media_asset.create.created_at_required", "CreatedAtUtc is required"));
            }

            return Result<MediaAsset>.Success(
                new MediaAsset(
                    Guid.NewGuid(),
                    ownerUserId,
                    storageProvider,
                    trimmedStorageObjectId,
                    deliveryUrlValidation.Value,
                    normalizedFileNameResult.Value,
                    trimmedContentType,
                    sizeBytes,
                    kind,
                    durationSeconds,
                    createdAtUtc));
        }

        private static Result<string> ValidateDeliveryUrl(
            MediaStorageProvider storageProvider,
            string deliveryUrl)
        {
            if (string.IsNullOrWhiteSpace(deliveryUrl))
            {
                return Result<string>.Failure(
                    Error.Validation("media_asset.create.delivery_url_required", "DeliveryUrl is required"));
            }

            var trimmedDeliveryUrl = deliveryUrl.Trim();
            if (trimmedDeliveryUrl.Length > MaxDeliveryUrlLength)
            {
                return Result<string>.Failure(
                    Error.Validation("media_asset.create.delivery_url_too_long", $"DeliveryUrl cannot exceed {MaxDeliveryUrlLength} characters"));
            }

            if (!Uri.TryCreate(trimmedDeliveryUrl, UriKind.Absolute, out var uri))
            {
                return Result<string>.Failure(
                    Error.Validation("media_asset.create.delivery_url_invalid", "DeliveryUrl must be an absolute URL"));
            }

            if (storageProvider == MediaStorageProvider.LocalDev)
            {
                if (uri.Scheme is not ("http" or "https"))
                {
                    return Result<string>.Failure(
                        Error.Validation("media_asset.create.delivery_url_invalid_scheme", "LocalDev DeliveryUrl must use http or https"));
                }
            }
            else if (uri.Scheme != "https")
            {
                return Result<string>.Failure(
                    Error.Validation("media_asset.create.delivery_url_invalid_scheme", "External provider DeliveryUrl must use https"));
            }

            return Result<string>.Success(trimmedDeliveryUrl);
        }

        private static Result<string?> NormalizeOptionalFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Result<string?>.Success(null);
            }

            var trimmedFileName = fileName.Trim();
            if (trimmedFileName.Length > MaxFileNameLength)
            {
                return Result<string?>.Failure(
                    Error.Validation("media_asset.create.file_name_too_long", $"FileName cannot exceed {MaxFileNameLength} characters"));
            }

            return Result<string?>.Success(trimmedFileName);
        }

        private static bool HasBasicMimeShape(string contentType)
        {
            var separatorIndex = contentType.IndexOf('/');

            return separatorIndex > 0 &&
                   separatorIndex < contentType.Length - 1;
        }

        private static bool MatchesKind(
            string contentType,
            MediaAssetKind kind)
        {
            return kind switch
            {
                MediaAssetKind.Image => contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase),
                MediaAssetKind.Video => contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase),
                MediaAssetKind.Audio => contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
    }
}
