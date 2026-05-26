using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public sealed class GetClientProgressPhotosHandler
        : IRequestHandler<GetClientProgressPhotosQuery, Result<IReadOnlyList<ClientProgressPhotoDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientProgressPhotoReadRepository _readRepository;

        public GetClientProgressPhotosHandler(
            ICurrentUserLoader currentUserLoader,
            IClientProgressPhotoReadRepository readRepository)
        {
            _currentUserLoader = currentUserLoader;
            _readRepository = readRepository;
        }

        public async Task<Result<IReadOnlyList<ClientProgressPhotoDto>>> Handle(
            GetClientProgressPhotosQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<ClientProgressPhotoDto>>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<IReadOnlyList<ClientProgressPhotoDto>>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var photos = await _readRepository.GetByClientAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            return Result<IReadOnlyList<ClientProgressPhotoDto>>.Success(photos);
        }
    }
}
