using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;
using FitLead.Application.Identity;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class UpdateExerciseHandler
    : IRequestHandler<UpdateExerciseCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateExerciseHandler(
            IUserContext user,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(
                request.ExerciseId,
                cancellationToken);

            if (exercise is null)
                return Result.Failure(Error.NotFound("exercise.not_found", "Exercise not found"));

            if (exercise.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("exercise.forbidden", "Forbidden"));

            var updateResult = exercise.Update(request.Name, request.Description, request.MediaUrl);
            if (updateResult.IsFailure)
                return updateResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
