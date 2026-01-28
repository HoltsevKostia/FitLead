using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class DeleteExerciseHandler
    : IRequestHandler<DeleteExerciseCommand, Result>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteExerciseHandler(
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(
                request.ExerciseId,
                cancellationToken);

            if (exercise is null)
                return Result.Failure("Exercise not found");

            if (exercise.TrainerId != request.TrainerId)
                return Result.Failure("Forbidden");

            _exerciseRepository.Remove(exercise);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
