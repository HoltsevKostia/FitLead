using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class GetWorkoutsByProgramIdHandler
        : IRequestHandler<GetWorkoutsByProgramIdQuery, Result<IReadOnlyList<TrainingProgramWorkoutDto>>>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly ITrainingProgramReadRepository _repository;

        public GetWorkoutsByProgramIdHandler(
            ITrainingProgramLoader programLoader,
            ITrainingProgramReadRepository trainingProgramReadRepository)
        {
            _programLoader = programLoader;
            _repository = trainingProgramReadRepository;
        }

        public async Task<Result<IReadOnlyList<TrainingProgramWorkoutDto>>> Handle(
            GetWorkoutsByProgramIdQuery request,
            CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.ProgramId,
                cancellationToken);

            if (programResult.IsFailure)
                return Result<IReadOnlyList<TrainingProgramWorkoutDto>>.Failure(
                    programResult.Error);

            var workouts = await _repository.GetWorkoutsByProgramIdAsync(
                request.ProgramId,
                cancellationToken);

            return Result<IReadOnlyList<TrainingProgramWorkoutDto>>.Success(workouts);
        }
    }
}
