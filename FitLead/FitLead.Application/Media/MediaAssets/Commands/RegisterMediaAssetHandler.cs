using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Media.MediaAssets.Queries;
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
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterMediaAssetHandler(
            IMediaAssetRepository mediaAssetRepository,
            ICurrentUserLoader currentUserLoader,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _mediaAssetRepository = mediaAssetRepository;
            _currentUserLoader = currentUserLoader;
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

            var kindResult = ParseKind(request.Kind);
            if (kindResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(kindResult.Error);
            }

            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(currentUserResult.Error);
            }

            var mediaAssetResult = MediaAsset.Create(
                currentUserResult.Value.Id,
                storageProviderResult.Value,
                request.StorageObjectId,
                request.DeliveryUrl,
                request.FileName,
                request.ContentType,
                request.SizeBytes,
                kindResult.Value,
                request.DurationSeconds,
                _clock.UtcNow);
            if (mediaAssetResult.IsFailure)
            {
                return Result<MediaAssetDto>.Failure(mediaAssetResult.Error);
            }

            var existingAsset = await _mediaAssetRepository.GetByStorageObjectAsync(
                mediaAssetResult.Value.StorageProvider,
                mediaAssetResult.Value.StorageObjectId,
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
    }
}
