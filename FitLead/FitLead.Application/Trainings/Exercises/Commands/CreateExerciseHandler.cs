using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Common.Results;
using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class CreateExerciseHandler
    : IRequestHandler<CreateExerciseCommand, Result<Guid>>
    {
        private readonly IUserContext _user;
        private readonly IUserRepository _userRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateExerciseHandler(
            IUserContext user,
            IUserRepository userRepository,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _userRepository = userRepository;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateExerciseCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(
                _user.UserId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure(Error.NotFound("trainer.not_found", "Trainer not found"));

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure(Error.Forbidden("trainer.required", "User is not a trainer"));

            var exerciseResult = Exercise.Create(
                _user.UserId,
                request.Name,
                request.Description,
                request.MediaUrl);
            if (exerciseResult.IsFailure)
                return Result<Guid>.Failure(exerciseResult.Error);

            await _exerciseRepository.AddAsync(exerciseResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(exerciseResult.Value.Id);
        }
    }
}
