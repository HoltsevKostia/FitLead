using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Media.MediaAssets.Access;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Clients.ProgressPhotos;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public sealed class CreateClientProgressPhotoHandler
        : IRequestHandler<CreateClientProgressPhotoCommand, Result<ClientProgressPhotoDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IMediaAssetLoader _mediaAssetLoader;
        private readonly IClientProgressPhotoRepository _repository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public CreateClientProgressPhotoHandler(
            ICurrentUserLoader currentUserLoader,
            IMediaAssetLoader mediaAssetLoader,
            IClientProgressPhotoRepository repository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _currentUserLoader = currentUserLoader;
            _mediaAssetLoader = mediaAssetLoader;
            _repository = repository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ClientProgressPhotoDto>> Handle(
            CreateClientProgressPhotoCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ClientProgressPhotoDto>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<ClientProgressPhotoDto>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var labelResult = EnumParser.ParseDefined<ProgressPhotoLabel>(
                request.Label,
                "client_progress_photo.create.label_required",
                "Label is required",
                "client_progress_photo.create.label_invalid",
                "Label is invalid");
            if (labelResult.IsFailure)
            {
                return Result<ClientProgressPhotoDto>.Failure(labelResult.Error);
            }

            var mediaAssetResult = await _mediaAssetLoader.GetOwnedOrNotFoundAsync(
                currentUserResult.Value.Id,
                request.MediaAssetId,
                cancellationToken);
            if (mediaAssetResult.IsFailure)
            {
                return Result<ClientProgressPhotoDto>.Failure(mediaAssetResult.Error);
            }

            var mediaAsset = mediaAssetResult.Value;
            if (mediaAsset.Status != MediaAssetStatus.Active)
            {
                return Result<ClientProgressPhotoDto>.Failure(
                    Error.Validation("media_asset.inactive", "Media asset is not active"));
            }

            if (mediaAsset.Kind != MediaAssetKind.Image)
            {
                return Result<ClientProgressPhotoDto>.Failure(
                    Error.Validation(
                        "media_asset.kind_not_allowed_for_progress_photo",
                        "Media asset kind is not allowed for progress photo"));
            }

            var createResult = ClientProgressPhoto.Create(
                currentUserResult.Value.Id,
                mediaAsset.Id,
                request.TakenAt,
                labelResult.Value,
                request.Note,
                _clock.UtcNow);
            if (createResult.IsFailure)
            {
                return Result<ClientProgressPhotoDto>.Failure(createResult.Error);
            }

            await _repository.AddAsync(createResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ClientProgressPhotoDto>.Success(
                ClientProgressPhotoMapping.ToDto(createResult.Value, mediaAsset));
        }
    }
}
