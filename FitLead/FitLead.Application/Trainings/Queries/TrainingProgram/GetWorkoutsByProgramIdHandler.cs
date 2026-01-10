using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Queries.Workout;
using MediatR;


namespace FitLead.Application.Trainings.Queries.TrainingProgram
{
    public sealed class GetWorkoutsByProgramIdHandler
        : IRequestHandler<GetWorkoutsByProgramIdQuery, IReadOnlyList<WorkoutDto>>
    {
        private readonly ITrainingProgramReadRepository _repository;
        public GetWorkoutsByProgramIdHandler(ITrainingProgramReadRepository trainingProgramReadRepository)
        {
            _repository = trainingProgramReadRepository;
        }

        public async Task<IReadOnlyList<WorkoutDto>> Handle(GetWorkoutsByProgramIdQuery request, CancellationToken cancellationToken)
        {
            var isOwner = await _repository.IsOwnedByTrainerAsync(
            request.ProgramId,
            request.TrainerId,
            cancellationToken);

            if (!isOwner)
                throw new UnauthorizedAccessException(
                    "Training program does not belong to this trainer");

            return await _repository.GetWorkoutsByProgramIdAsync(
                request.ProgramId,
                cancellationToken);
        }      
    }
}

