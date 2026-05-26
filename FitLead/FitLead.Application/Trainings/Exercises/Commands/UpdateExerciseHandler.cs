using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Identity;
using FitLead.Application.Media.MediaAssets.Access;
using FitLead.Application.Trainings.Exercises.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class UpdateExerciseHandler
    : IRequestHandler<UpdateExerciseCommand, Result>
    {
        private readonly IExerciseLoader _exerciseLoader;
        private readonly IUserContext _userContext;
        private readonly IMediaAssetLoader _mediaAssetLoader;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateExerciseHandler(
            IExerciseLoader exerciseLoader,
            IUserContext userContext,
            IMediaAssetLoader mediaAssetLoader,
            IUnitOfWork unitOfWork)
        {
            _exerciseLoader = exerciseLoader;
            _userContext = userContext;
            _mediaAssetLoader = mediaAssetLoader;
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

            if (request.MediaAssetId.HasValue)
            {
                var mediaAssetResult = await _mediaAssetLoader.GetOwnedAllowedForExerciseOrNotFoundAsync(
                    _userContext.UserId,
                    request.MediaAssetId.Value,
                    cancellationToken);
                if (mediaAssetResult.IsFailure)
                {
                    return Result.Failure(mediaAssetResult.Error);
                }
            }

            var updateResult = exercise.UpdateByTrainer(
                _userContext.UserId,
                request.Name,
                request.Description,
                request.MediaAssetId,
                request.MuscleGroup,
                request.Equipment);
            if (updateResult.IsFailure)
                return updateResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
