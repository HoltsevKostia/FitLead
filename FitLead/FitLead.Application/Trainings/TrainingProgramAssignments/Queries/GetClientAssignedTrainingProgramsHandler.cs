using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed class GetClientAssignedTrainingProgramsHandler
        : IRequestHandler<GetClientAssignedTrainingProgramsQuery, Result<IReadOnlyList<ClientAssignedTrainingProgramDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IAssignedTrainingProgramReadRepository _assignmentReadRepository;
        private readonly IClock _clock;

        public GetClientAssignedTrainingProgramsHandler(
            ICurrentUserLoader currentUserLoader,
            IAssignedTrainingProgramReadRepository assignmentReadRepository,
            IClock clock)
        {
            _currentUserLoader = currentUserLoader;
            _assignmentReadRepository = assignmentReadRepository;
            _clock = clock;
        }

        public async Task<Result<IReadOnlyList<ClientAssignedTrainingProgramDto>>> Handle(
            GetClientAssignedTrainingProgramsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<ClientAssignedTrainingProgramDto>>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<IReadOnlyList<ClientAssignedTrainingProgramDto>>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var programs = await _assignmentReadRepository.GetAccessibleByClientIdAsync(
                currentUserResult.Value.Id,
                _clock.UtcNow,
                cancellationToken);

            return Result<IReadOnlyList<ClientAssignedTrainingProgramDto>>.Success(programs);
        }
    }
}
