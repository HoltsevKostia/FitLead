using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using FitLead.Application.Trainings.Workouts.Queries;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class GetWorkoutsByProgramIdHandler
        : IRequestHandler<GetWorkoutsByProgramIdQuery, Result<IReadOnlyList<WorkoutDto>>>
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

        public async Task<Result<IReadOnlyList<WorkoutDto>>> Handle(GetWorkoutsByProgramIdQuery request, CancellationToken cancellationToken)
        {
            var isOwner = await _repository.IsOwnedByTrainerAsync(
            request.ProgramId,
            _user.UserId,
            cancellationToken);

            if (!isOwner)
                return Result<IReadOnlyList<WorkoutDto>>.Failure(
                    Error.Forbidden("training_program.forbidden", "Forbidden"));

            var workouts = await _repository.GetWorkoutsByProgramIdAsync(
                request.ProgramId,
                cancellationToken);
            return Result<IReadOnlyList<WorkoutDto>>.Success(workouts);
        }      
    }
}

