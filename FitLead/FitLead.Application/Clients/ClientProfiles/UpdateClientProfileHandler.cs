using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Clients.ClientProfiles;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.ClientProfiles
{
    public sealed class UpdateClientProfileHandler
        : IRequestHandler<UpdateClientProfileCommand, Result<ClientProfileDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientProfileRepository _clientProfileRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateClientProfileHandler(
            ICurrentUserLoader currentUserLoader,
            IClientProfileRepository clientProfileRepository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _currentUserLoader = currentUserLoader;
            _clientProfileRepository = clientProfileRepository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ClientProfileDto>> Handle(
            UpdateClientProfileCommand request,
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

            var experienceLevelResult = EnumParser.ParseOptionalDefined<ClientExperienceLevel>(
                request.ExperienceLevel,
                "client_profile.experience_level_invalid",
                "ExperienceLevel is invalid");
            if (experienceLevelResult.IsFailure)
            {
                return Result<ClientProfileDto>.Failure(experienceLevelResult.Error);
            }

            var utcNow = _clock.UtcNow;
            var profile = await _clientProfileRepository.GetByClientIdAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            if (profile is null)
            {
                var createResult = ClientProfile.Create(
                    currentUserResult.Value.Id,
                    request.Goal,
                    experienceLevelResult.Value.Value,
                    request.HeightCm,
                    request.Limitations,
                    request.TrainingPreferences,
                    request.AdditionalInfo,
                    utcNow);
                if (createResult.IsFailure)
                {
                    return Result<ClientProfileDto>.Failure(createResult.Error);
                }

                await _clientProfileRepository.AddAsync(createResult.Value, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<ClientProfileDto>.Success(ClientProfileMapping.ToDto(createResult.Value));
            }

            var updateResult = profile.Update(
                request.Goal,
                experienceLevelResult.Value.Value,
                request.HeightCm,
                request.Limitations,
                request.TrainingPreferences,
                request.AdditionalInfo,
                utcNow);
            if (updateResult.IsFailure)
            {
                return Result<ClientProfileDto>.Failure(updateResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ClientProfileDto>.Success(ClientProfileMapping.ToDto(profile));
        }
    }
}
