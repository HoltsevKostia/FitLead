using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Trainings.Exercises.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class UpdateExerciseHandler
    : IRequestHandler<UpdateExerciseCommand, Result>
    {
        private readonly IExerciseLoader _exerciseLoader;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateExerciseHandler(
            IExerciseLoader exerciseLoader,
            IUnitOfWork unitOfWork)
        {
            _exerciseLoader = exerciseLoader;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
        {
            var exerciseResult = await _exerciseLoader.GetOwnedOrNotFoundAsync(
                request.ExerciseId,
                cancellationToken);

            if (exerciseResult.IsFailure)
                return Result.Failure(exerciseResult.Error);

            var exercise = exerciseResult.Value;
            var updateResult = exercise.Update(request.Name, request.Description, request.MediaUrl);
            if (updateResult.IsFailure)
                return updateResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
