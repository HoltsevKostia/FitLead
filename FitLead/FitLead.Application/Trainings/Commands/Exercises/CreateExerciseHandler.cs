using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Commands.Exercises
{
    public sealed class CreateExerciseHandler
    : IRequestHandler<CreateExerciseCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateExerciseHandler(
            IUserRepository userRepository,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateExerciseCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(
                request.TrainerId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure("Trainer not found");

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure("User is not a trainer");

            var exercise = Exercise.Create(
                request.TrainerId,
                request.Name,
                request.Description,
                request.MediaUrl);

            await _exerciseRepository.AddAsync(exercise, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(exercise.Id);
        }
    }
}
