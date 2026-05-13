using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed class GetTrainingProgramAssignmentsHandler
        : IRequestHandler<GetTrainingProgramAssignmentsQuery, Result<IReadOnlyList<TrainingProgramAssignmentDto>>>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly IAssignedTrainingProgramReadRepository _assignmentReadRepository;

        public GetTrainingProgramAssignmentsHandler(
            ITrainingProgramLoader programLoader,
            IAssignedTrainingProgramReadRepository assignmentReadRepository)
        {
            _programLoader = programLoader;
            _assignmentReadRepository = assignmentReadRepository;
        }

        public async Task<Result<IReadOnlyList<TrainingProgramAssignmentDto>>> Handle(
            GetTrainingProgramAssignmentsQuery request,
            CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.TrainingProgramId,
                cancellationToken);
            if (programResult.IsFailure)
            {
                return Result<IReadOnlyList<TrainingProgramAssignmentDto>>.Failure(programResult.Error);
            }

            var assignments = await _assignmentReadRepository.GetByProgramIdAsync(
                programResult.Value.Id,
                cancellationToken);

            return Result<IReadOnlyList<TrainingProgramAssignmentDto>>.Success(assignments);
        }
    }
}
