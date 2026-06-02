using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Clients.ClientProfiles;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientProfileHandler
        : IRequestHandler<GetTrainerClientProfileQuery, Result<ClientProfileDto>>
    {
        private readonly ITrainerClientAccessLoader _accessLoader;
        private readonly IClientProfileRepository _clientProfileRepository;

        public GetTrainerClientProfileHandler(
            ITrainerClientAccessLoader accessLoader,
            IClientProfileRepository clientProfileRepository)
        {
            _accessLoader = accessLoader;
            _clientProfileRepository = clientProfileRepository;
        }

        public async Task<Result<ClientProfileDto>> Handle(
            GetTrainerClientProfileQuery request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _accessLoader.GetAccessibleClientAsync(
                request.ClientId,
                cancellationToken);

            if (accessResult.IsFailure)
            {
                return Result<ClientProfileDto>.Failure(accessResult.Error);
            }

            var profile = await _clientProfileRepository.GetByClientIdAsync(
                accessResult.Value.ClientId,
                cancellationToken);

            return Result<ClientProfileDto>.Success(
                profile is null
                    ? new ClientProfileDto(
                        accessResult.Value.ClientId,
                        Goal: null,
                        ExperienceLevel: null,
                        HeightCm: null,
                        Limitations: null,
                        TrainingPreferences: null,
                        AdditionalInfo: null,
                        CreatedAtUtc: null,
                        UpdatedAtUtc: null)
                    : new ClientProfileDto(
                        profile.ClientId,
                        profile.Goal,
                        profile.ExperienceLevel?.ToString(),
                        profile.HeightCm,
                        profile.Limitations,
                        profile.TrainingPreferences,
                        profile.AdditionalInfo,
                        profile.CreatedAtUtc,
                        profile.UpdatedAtUtc));
        }
    }
}
