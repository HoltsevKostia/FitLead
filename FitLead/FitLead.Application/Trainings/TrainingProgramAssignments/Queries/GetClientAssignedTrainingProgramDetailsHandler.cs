using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed class GetClientAssignedTrainingProgramDetailsHandler
        : IRequestHandler<GetClientAssignedTrainingProgramDetailsQuery, Result<ClientAssignedTrainingProgramDetailsDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IAssignedTrainingProgramReadRepository _assignmentReadRepository;
        private readonly IClock _clock;

        public GetClientAssignedTrainingProgramDetailsHandler(
            ICurrentUserLoader currentUserLoader,
            IAssignedTrainingProgramReadRepository assignmentReadRepository,
            IClock clock)
        {
            _currentUserLoader = currentUserLoader;
            _assignmentReadRepository = assignmentReadRepository;
            _clock = clock;
        }

        public async Task<Result<ClientAssignedTrainingProgramDetailsDto>> Handle(
            GetClientAssignedTrainingProgramDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ClientAssignedTrainingProgramDetailsDto>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<ClientAssignedTrainingProgramDetailsDto>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var details = await _assignmentReadRepository.GetAccessibleDetailsByAssignmentIdAsync(
                request.AssignmentId,
                currentUserResult.Value.Id,
                _clock.UtcNow,
                cancellationToken);

            if (details is null)
            {
                return Result<ClientAssignedTrainingProgramDetailsDto>.Failure(
                    Error.NotFound("training_program.assignment.not_found", "Assignment not found"));
            }

            return Result<ClientAssignedTrainingProgramDetailsDto>.Success(details);
        }
    }
}
