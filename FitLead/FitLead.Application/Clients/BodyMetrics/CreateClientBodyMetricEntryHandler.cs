using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Clients.BodyMetrics;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.BodyMetrics
{
    public sealed class CreateClientBodyMetricEntryHandler
        : IRequestHandler<CreateClientBodyMetricEntryCommand, Result<ClientBodyMetricEntryDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientBodyMetricEntryRepository _repository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public CreateClientBodyMetricEntryHandler(
            ICurrentUserLoader currentUserLoader,
            IClientBodyMetricEntryRepository repository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _currentUserLoader = currentUserLoader;
            _repository = repository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ClientBodyMetricEntryDto>> Handle(
            CreateClientBodyMetricEntryCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ClientBodyMetricEntryDto>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<ClientBodyMetricEntryDto>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var clientId = currentUserResult.Value.Id;
            var duplicateExists = await _repository.ExistsForClientRecordedAtAsync(
                clientId,
                request.RecordedAt,
                excludeEntryId: null,
                cancellationToken);
            if (duplicateExists)
            {
                return Result<ClientBodyMetricEntryDto>.Failure(
                    Error.Conflict(
                        "body_metric_entry.recorded_at_conflict",
                        "Body metric entry already exists for this date"));
            }

            var createResult = ClientBodyMetricEntry.Create(
                clientId,
                request.RecordedAt,
                request.WeightKg,
                request.BodyFatPercent,
                request.ChestCm,
                request.WaistCm,
                request.HipsCm,
                request.ArmCm,
                request.ThighCm,
                request.Note,
                _clock.UtcNow);
            if (createResult.IsFailure)
            {
                return Result<ClientBodyMetricEntryDto>.Failure(createResult.Error);
            }

            await _repository.AddAsync(createResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ClientBodyMetricEntryDto>.Success(
                ClientBodyMetricEntryMapping.ToDto(createResult.Value));
        }
    }
}
