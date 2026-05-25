using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public sealed class DeleteClientProgressPhotoHandler
        : IRequestHandler<DeleteClientProgressPhotoCommand, Result>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientProgressPhotoRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteClientProgressPhotoHandler(
            ICurrentUserLoader currentUserLoader,
            IClientProgressPhotoRepository repository,
            IUnitOfWork unitOfWork)
        {
            _currentUserLoader = currentUserLoader;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeleteClientProgressPhotoCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var photo = await _repository.GetByIdForClientAsync(
                request.PhotoId,
                currentUserResult.Value.Id,
                cancellationToken);
            if (photo is null)
            {
                return Result.Failure(
                    Error.NotFound(
                        "client_progress_photo.not_found",
                        "Progress photo not found"));
            }

            _repository.Remove(photo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
