using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
using FitLead.Application.Trainings.Workouts.Queries;
using MediatR;


namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class GetWorkoutsByProgramIdHandler
        : IRequestHandler<GetWorkoutsByProgramIdQuery, IReadOnlyList<WorkoutDto>>
    {
        private readonly IUserContext _user;
        private readonly ITrainingProgramReadRepository _repository;
        public GetWorkoutsByProgramIdHandler(
            IUserContext user,
            ITrainingProgramReadRepository trainingProgramReadRepository)
        {
            _user = user;
            _repository = trainingProgramReadRepository;
        }

        public async Task<IReadOnlyList<WorkoutDto>> Handle(GetWorkoutsByProgramIdQuery request, CancellationToken cancellationToken)
        {
            var isOwner = await _repository.IsOwnedByTrainerAsync(
            request.ProgramId,
            _user.UserId,
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

