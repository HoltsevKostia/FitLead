using FitLead.Application.Abstractions.Persistence;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;

namespace FitLead.Application.Users.Access
{
    public sealed class TrainerClientAccessLoader : ITrainerClientAccessLoader
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly ITrainerClientReadRepository _trainerClientReadRepository;

        public TrainerClientAccessLoader(
            ICurrentUserLoader currentUserLoader,
            ITrainerClientReadRepository trainerClientReadRepository)
        {
            _currentUserLoader = currentUserLoader;
            _trainerClientReadRepository = trainerClientReadRepository;
        }

        public async Task<Result<TrainerClientAccessContext>> GetAccessibleClientAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<TrainerClientAccessContext>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Trainer)
            {
                return Result<TrainerClientAccessContext>.Failure(
                    Error.Forbidden("trainer.required", "User is not a trainer"));
            }

            var client = await _trainerClientReadRepository.GetClientByTrainerIdAndClientIdAsync(
                currentUserResult.Value.Id,
                clientId,
                cancellationToken);

            if (client is null)
            {
                return Result<TrainerClientAccessContext>.Failure(
                    Error.NotFound(
                        "trainer_client.not_found",
                        "Trainer client relationship was not found"));
            }

            return Result<TrainerClientAccessContext>.Success(
                new TrainerClientAccessContext(
                    currentUserResult.Value.Id,
                    client.ClientId,
                    client.Email,
                    client.FullName));
        }
    }
}
