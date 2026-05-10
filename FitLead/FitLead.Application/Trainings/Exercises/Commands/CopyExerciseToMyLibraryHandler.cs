using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Trainings.Exercises;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class CopyExerciseToMyLibraryHandler
        : IRequestHandler<CopyExerciseToMyLibraryCommand, Result<Guid>>
    {
        private readonly IUserContext _userContext;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CopyExerciseToMyLibraryHandler(
            IUserContext userContext,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _userContext = userContext;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CopyExerciseToMyLibraryCommand request,
            CancellationToken cancellationToken)
        {
            var sourceExercise = await _exerciseRepository.GetByIdAsync(
                request.ExerciseId,
                cancellationToken);

            if (sourceExercise is null)
                return Result<Guid>.Failure(Error.NotFound("exercise.not_found", "Exercise not found"));

            if (sourceExercise.Source == ExerciseSource.Trainer
                && sourceExercise.OwnerTrainerId != _userContext.UserId)
            {
                return Result<Guid>.Failure(Error.NotFound("exercise.not_found", "Exercise not found"));
            }

            if (sourceExercise.Source != ExerciseSource.Platform)
            {
                return Result<Guid>.Failure(Error.Validation(
                    "exercise.copy.source_must_be_platform",
                    "Only platform exercises can be copied to trainer library"));
            }

            var copyExists = await _exerciseRepository.TrainerCopyExistsAsync(
                _userContext.UserId,
                sourceExercise.Id,
                cancellationToken);

            if (copyExists)
            {
                return Result<Guid>.Failure(Error.Conflict(
                    "exercise.copy.already_exists",
                    "Trainer already has a copy of this platform exercise"));
            }

            var copyResult = Exercise.CopyFromPlatformExercise(
                _userContext.UserId,
                sourceExercise);

            if (copyResult.IsFailure)
                return Result<Guid>.Failure(copyResult.Error);

            await _exerciseRepository.AddAsync(copyResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(copyResult.Value.Id);
        }
    }
}
