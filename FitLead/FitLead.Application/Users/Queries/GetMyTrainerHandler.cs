using FitLead.Application.Modules.Users;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetMyTrainerHandler
        : IRequestHandler<GetMyTrainerQuery, Result<ClientTrainerDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IUsersModule _usersModule;

        public GetMyTrainerHandler(
            ICurrentUserLoader currentUserLoader,
            IUsersModule usersModule)
        {
            _currentUserLoader = currentUserLoader;
            _usersModule = usersModule;
        }

        public async Task<Result<ClientTrainerDto>> Handle(
            GetMyTrainerQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ClientTrainerDto>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<ClientTrainerDto>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var trainerId = await _usersModule.GetActiveTrainerIdForClientAsync(
                currentUserResult.Value.Id,
                cancellationToken);
            if (!trainerId.HasValue)
            {
                return Result<ClientTrainerDto>.Failure(
                    Error.NotFound("trainer.not_found", "Trainer not found"));
            }

            var trainerProfile = await _usersModule.GetTrainerPublicProfileAsync(
                trainerId.Value,
                cancellationToken);
            if (trainerProfile is null)
            {
                return Result<ClientTrainerDto>.Failure(
                    Error.NotFound("trainer.not_found", "Trainer not found"));
            }

            return Result<ClientTrainerDto>.Success(
                new ClientTrainerDto(
                    trainerId.Value,
                    trainerProfile.FullName));
        }
    }
}
