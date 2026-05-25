using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.ClientProfiles
{
    public sealed class GetClientProfileHandler
        : IRequestHandler<GetClientProfileQuery, Result<ClientProfileDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientProfileRepository _clientProfileRepository;

        public GetClientProfileHandler(
            ICurrentUserLoader currentUserLoader,
            IClientProfileRepository clientProfileRepository)
        {
            _currentUserLoader = currentUserLoader;
            _clientProfileRepository = clientProfileRepository;
        }

        public async Task<Result<ClientProfileDto>> Handle(
            GetClientProfileQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ClientProfileDto>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<ClientProfileDto>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var profile = await _clientProfileRepository.GetByClientIdAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            return Result<ClientProfileDto>.Success(
                profile is null
                    ? ClientProfileMapping.Empty(currentUserResult.Value.Id)
                    : ClientProfileMapping.ToDto(profile));
        }
    }
}
