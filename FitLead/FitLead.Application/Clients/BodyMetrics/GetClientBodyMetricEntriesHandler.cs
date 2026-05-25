using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.BodyMetrics
{
    public sealed class GetClientBodyMetricEntriesHandler
        : IRequestHandler<GetClientBodyMetricEntriesQuery, Result<IReadOnlyList<ClientBodyMetricEntryDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientBodyMetricEntryReadRepository _readRepository;

        public GetClientBodyMetricEntriesHandler(
            ICurrentUserLoader currentUserLoader,
            IClientBodyMetricEntryReadRepository readRepository)
        {
            _currentUserLoader = currentUserLoader;
            _readRepository = readRepository;
        }

        public async Task<Result<IReadOnlyList<ClientBodyMetricEntryDto>>> Handle(
            GetClientBodyMetricEntriesQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<ClientBodyMetricEntryDto>>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<IReadOnlyList<ClientBodyMetricEntryDto>>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var entries = await _readRepository.GetByClientAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            return Result<IReadOnlyList<ClientBodyMetricEntryDto>>.Success(entries);
        }
    }
}
