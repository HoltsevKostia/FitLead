using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Results;
using MediatR;


namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class UpdateExerciseHandler
    : IRequestHandler<UpdateExerciseCommand, Result>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateExerciseHandler(
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(
                request.ExerciseId,
                cancellationToken);

            if (exercise is null)
                return Result.Failure("Exercise not found");

            if (exercise.TrainerId != request.TrainerId)
                return Result.Failure("Forbidden");

            exercise.Update(request.Name, request.Description, request.MediaUrl);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
