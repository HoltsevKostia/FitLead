using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.Application.Media.MediaAssets.Registration;
using FitLead.Application.Media.Uploadcare;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Media.MediaAssets;
using MediatR;

namespace FitLead.Application.Media.MediaAssets.Commands
{
    public sealed class RegisterMediaAssetHandler
        : IRequestHandler<RegisterMediaAssetCommand, Result<MediaAssetDto>>
    {
        private readonly IMediaAssetRepository _mediaAssetRepository;
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IMediaAssetRegistrationPolicy _registrationPolicy;
        private readonly IUploadcareClient _uploadcareClient;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterMediaAssetHandler(
            IMediaAssetRepository mediaAssetRepository,
            ICurrentUserLoader currentUserLoader,
            IMediaAssetRegistrationPolicy registrationPolicy,
            IUploadcareClient uploadcareClient,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _mediaAssetRepository = mediaAssetRepository;
            _currentUserLoader = currentUserLoader;
            _registrationPolicy = registrationPolicy;
            _uploadcareClient = uploadcareClient;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<MediaAssetDto>> Handle(
            RegisterMediaAssetCommand request,
            CancellationToken cancellationToken)
        {
            var storageProviderResult = ParseStorageProvider(request.StorageProvider);
            if (storageProviderResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(storageProviderResult.Error);
            }

            if (!_registrationPolicy.IsProviderAllowed(storageProviderResult.Value))
            {
                return Result<MediaAssetDto>.Failure(
                    Error.Validation(
                        "media_asset.storage_provider_not_allowed",
                        "StorageProvider is not allowed"));
            }

            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(currentUserResult.Error);
            }

            var normalizedStorageObjectIdResult = NormalizeStorageObjectId(request.StorageObjectId);
            if (normalizedStorageObjectIdResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(normalizedStorageObjectIdResult.Error);
            }

            if (storageProviderResult.Value == MediaStorageProvider.Uploadcare &&
                !Guid.TryParse(normalizedStorageObjectIdResult.Value, out _))
            {
                return Result<MediaAssetDto>.Failure(
                    Error.Validation(
                        "media_asset.uploadcare_storage_object_id_invalid",
                        "Uploadcare StorageObjectId must be a UUID"));
            }

            var existingAsset = await _mediaAssetRepository.GetByStorageObjectAsync(
                storageProviderResult.Value,
                normalizedStorageObjectIdResult.Value,
                cancellationToken);
            if (existingAsset is not null)
            {
                if (existingAsset.OwnerUserId == currentUserResult.Value.Id)
                {
                    return Result<MediaAssetDto>.Success(ToDto(existingAsset));
                }

                return Result<MediaAssetDto>.Failure(
                    Error.Conflict(
                        "media_asset.storage_object_already_registered",
                        "Storage object is already registered"));
            }

            var metadataResult = await GetRegistrationMetadataAsync(
                request,
                storageProviderResult.Value,
                normalizedStorageObjectIdResult.Value,
                cancellationToken);
            if (metadataResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(metadataResult.Error);
            }

            var mediaAssetResult = MediaAsset.Create(
                currentUserResult.Value.Id,
                storageProviderResult.Value,
                metadataResult.Value.StorageObjectId,
                metadataResult.Value.DeliveryUrl,
                metadataResult.Value.FileName,
                metadataResult.Value.ContentType,
                metadataResult.Value.SizeBytes,
                metadataResult.Value.Kind,
                metadataResult.Value.DurationSeconds,
                _clock.UtcNow);
            if (mediaAssetResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(mediaAssetResult.Error);
            }

            await _mediaAssetRepository.AddAsync(mediaAssetResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<MediaAssetDto>.Success(ToDto(mediaAssetResult.Value));
        }

        private static Result<MediaStorageProvider> ParseStorageProvider(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<MediaStorageProvider>.Failure(
                    Error.Validation("media_asset.storage_provider_required", "StorageProvider is required"));
            }

            if (!Enum.TryParse<MediaStorageProvider>(value.Trim(), true, out var storageProvider) ||
                !Enum.IsDefined(storageProvider))
            {
                return Result<MediaStorageProvider>.Failure(
                    Error.Validation("media_asset.storage_provider_invalid", "StorageProvider is invalid"));
            }

            return Result<MediaStorageProvider>.Success(storageProvider);
        }

        private static Result<MediaAssetKind> ParseKind(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<MediaAssetKind>.Failure(
                    Error.Validation("media_asset.kind_required", "Kind is required"));
            }

            if (!Enum.TryParse<MediaAssetKind>(value.Trim(), true, out var kind) ||
                !Enum.IsDefined(kind))
            {
                return Result<MediaAssetKind>.Failure(
                    Error.Validation("media_asset.kind_invalid", "Kind is invalid"));
            }

            return Result<MediaAssetKind>.Success(kind);
        }

        private async Task<Result<RegistrationMetadata>> GetRegistrationMetadataAsync(
            RegisterMediaAssetCommand request,
            MediaStorageProvider storageProvider,
            string normalizedStorageObjectId,
            CancellationToken cancellationToken)
        {
            if (storageProvider == MediaStorageProvider.Uploadcare)
            {
                UploadcareFileInfo? fileInfo;
                try
                {
                    fileInfo = await _uploadcareClient.GetFileInfoAsync(
                        normalizedStorageObjectId,
                        cancellationToken);
                }
                catch (Exception)
                {
                    return Result<RegistrationMetadata>.Failure(
                        Error.Failure(
                            "media_asset.uploadcare_verification_failed",
                            "Could not verify Uploadcare file"));
                }
                if (fileInfo is null)
                {
                    return Result<RegistrationMetadata>.Failure(
                        Error.Validation(
                            "media_asset.uploadcare_file_not_found",
                            "Uploadcare file was not found"));
                }

                var verifiedKindResult = GetKindFromContentType(fileInfo.MimeType);
                if (verifiedKindResult.IsFailure)
                {
                    return Result<RegistrationMetadata>.Failure(verifiedKindResult.Error);
                }

                return Result<RegistrationMetadata>.Success(
                    new RegistrationMetadata(
                        fileInfo.Uuid,
                        fileInfo.OriginalFileUrl,
                        fileInfo.OriginalFilename,
                        fileInfo.MimeType,
                        fileInfo.Size,
                        verifiedKindResult.Value,
                        ToDurationSeconds(fileInfo.DurationMilliseconds)));
            }

            var kindResult = ParseKind(request.Kind);
            if (kindResult.IsFailure)
            {
                return Result<RegistrationMetadata>.Failure(kindResult.Error);
            }

            return Result<RegistrationMetadata>.Success(
                new RegistrationMetadata(
                    normalizedStorageObjectId,
                    request.DeliveryUrl,
                    request.FileName,
                    request.ContentType,
                    request.SizeBytes,
                    kindResult.Value,
                    request.DurationSeconds));
        }

        private static Result<string> NormalizeStorageObjectId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<string>.Failure(
                    Error.Validation(
                        "media_asset.storage_object_id_required",
                        "StorageObjectId is required"));
            }

            var normalizedValue = value.Trim();
            if (normalizedValue.Length > MediaAsset.MaxStorageObjectIdLength)
            {
                return Result<string>.Failure(
                    Error.Validation(
                        "media_asset.storage_object_id_too_long",
                        $"StorageObjectId cannot exceed {MediaAsset.MaxStorageObjectIdLength} characters"));
            }

            return Result<string>.Success(normalizedValue);
        }

        private static Result<MediaAssetKind> GetKindFromContentType(string contentType)
        {
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Result<MediaAssetKind>.Success(MediaAssetKind.Image);
            }

            if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return Result<MediaAssetKind>.Success(MediaAssetKind.Video);
            }

            if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                return Result<MediaAssetKind>.Success(MediaAssetKind.Audio);
            }

            return Result<MediaAssetKind>.Failure(
                Error.Validation(
                    "media_asset.uploadcare_content_type_unsupported",
                    "Uploadcare file content type is unsupported"));
        }

        private static int? ToDurationSeconds(int? durationMilliseconds)
        {
            if (!durationMilliseconds.HasValue || durationMilliseconds.Value <= 0)
            {
                return null;
            }

            return (int)Math.Ceiling(durationMilliseconds.Value / 1000d);
        }

        private static MediaAssetDto ToDto(MediaAsset mediaAsset)
        {
            return new MediaAssetDto(
                mediaAsset.Id,
                mediaAsset.StorageProvider.ToString(),
                mediaAsset.StorageObjectId,
                mediaAsset.DeliveryUrl,
                mediaAsset.FileName,
                mediaAsset.ContentType,
                mediaAsset.SizeBytes,
                mediaAsset.Kind.ToString(),
                mediaAsset.DurationSeconds,
                mediaAsset.Status.ToString(),
                mediaAsset.CreatedAtUtc);
        }

        private sealed record RegistrationMetadata(
            string StorageObjectId,
            string DeliveryUrl,
            string? FileName,
            string ContentType,
            long SizeBytes,
            MediaAssetKind Kind,
            int? DurationSeconds);
    }
}
