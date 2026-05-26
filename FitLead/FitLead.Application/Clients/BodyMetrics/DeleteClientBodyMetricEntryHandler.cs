using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Clients.BodyMetrics
{
    public sealed class DeleteClientBodyMetricEntryHandler
        : IRequestHandler<DeleteClientBodyMetricEntryCommand, Result>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientBodyMetricEntryRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteClientBodyMetricEntryHandler(
            ICurrentUserLoader currentUserLoader,
            IClientBodyMetricEntryRepository repository,
            IUnitOfWork unitOfWork)
        {
            _currentUserLoader = currentUserLoader;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeleteClientBodyMetricEntryCommand request,
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

            var entry = await _repository.GetByIdForClientAsync(
                request.EntryId,
                currentUserResult.Value.Id,
                cancellationToken);
            if (entry is null)
            {
                return Result.Failure(
                    Error.NotFound("body_metric_entry.not_found", "Body metric entry not found"));
            }

            _repository.Remove(entry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
