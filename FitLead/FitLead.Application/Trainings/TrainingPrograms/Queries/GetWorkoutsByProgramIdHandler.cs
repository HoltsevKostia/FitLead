using FitLead.Application.Abstractions.Persistence;
using FitLead.Common.Results;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Application.Trainings.Workouts.Queries;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class GetWorkoutsByProgramIdHandler
        : IRequestHandler<GetWorkoutsByProgramIdQuery, Result<IReadOnlyList<WorkoutDto>>>
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

        public async Task<Result<IReadOnlyList<WorkoutDto>>> Handle(GetWorkoutsByProgramIdQuery request, CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.ProgramId,
                cancellationToken);

            if (programResult.IsFailure)
                return Result<IReadOnlyList<WorkoutDto>>.Failure(
                    programResult.Error);

            var workouts = await _repository.GetWorkoutsByProgramIdAsync(
                request.ProgramId,
                cancellationToken);
            return Result<IReadOnlyList<WorkoutDto>>.Success(workouts);
        }      
    }
}

