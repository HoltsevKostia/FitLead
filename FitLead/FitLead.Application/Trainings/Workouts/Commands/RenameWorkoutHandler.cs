using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Workouts.Access;
using FitLead.Application.Common;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class RenameWorkoutHandler
    : IRequestHandler<RenameWorkoutCommand, Result>
    {
        private readonly IWorkoutLoader _workoutLoader;
        private readonly IUnitOfWork _unitOfWork;

        public RenameWorkoutHandler(
            IWorkoutLoader workoutLoader,
            IUnitOfWork unitOfWork)
        {
            _workoutLoader = workoutLoader;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RenameWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workoutResult = await _workoutLoader.GetOwnedOrNotFoundAsync(
                request.WorkoutId,
                cancellationToken);

            if (workoutResult.IsFailure)
                return Result.Failure(workoutResult.Error);

            var workout = workoutResult.Value;
            var renameResult = workout.Rename(request.Name);
            if (renameResult.IsFailure)
                return renameResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
