using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Trainings.Commands.Workouts
{
    public sealed class RenameWorkoutHandler
    : IRequestHandler<RenameWorkoutCommand, Result>
    {
        private readonly IWorkoutRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public RenameWorkoutHandler(
            IWorkoutRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RenameWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workout = await _repository.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result.Failure("Workout not found");

            workout.Rename(request.Name);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
