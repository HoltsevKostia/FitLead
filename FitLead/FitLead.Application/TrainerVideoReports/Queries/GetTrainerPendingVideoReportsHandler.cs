using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.TrainerVideoReports.Queries
{
    public sealed class GetTrainerPendingVideoReportsHandler
        : IRequestHandler<
            GetTrainerPendingVideoReportsQuery,
            Result<IReadOnlyList<TrainerPendingVideoReportDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly ITrainerPendingVideoReportsReadRepository _readRepository;

        public GetTrainerPendingVideoReportsHandler(
            ICurrentUserLoader currentUserLoader,
            ITrainerPendingVideoReportsReadRepository readRepository)
        {
            _currentUserLoader = currentUserLoader;
            _readRepository = readRepository;
        }

        public async Task<Result<IReadOnlyList<TrainerPendingVideoReportDto>>> Handle(
            GetTrainerPendingVideoReportsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult =
                await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<TrainerPendingVideoReportDto>>.Failure(
                    currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Trainer)
            {
                return Result<IReadOnlyList<TrainerPendingVideoReportDto>>.Failure(
                    Error.Forbidden("trainer.required", "User is not a trainer"));
            }

            var reports = await _readRepository.GetPendingAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            return Result<IReadOnlyList<TrainerPendingVideoReportDto>>.Success(reports);
        }
    }
}
