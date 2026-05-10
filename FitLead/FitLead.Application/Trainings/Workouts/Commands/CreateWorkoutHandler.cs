using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Modules.Users;
using FitLead.Domain.Users;
using MediatR;
using FitLead.Application.Identity;
using FitLead.Domain.Trainings.Workouts;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class CreateWorkoutHandler
    : IRequestHandler<CreateWorkoutCommand, Result<Guid>>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutRepository _repository;
        private readonly IUsersModule _usersModule;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWorkoutHandler(
            IUserContext user,
            IWorkoutRepository repository,
            IUsersModule usersModule,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _repository = repository;
            _usersModule = usersModule;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _usersModule.GetByIdAsync(
                _user.UserId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure(Error.NotFound("trainer.not_found", "Trainer not found"));

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure(Error.Forbidden("trainer.required", "User is not a trainer"));

            var workoutResult = Workout.Create(
                request.Name,
                _user.UserId);
            if (workoutResult.IsFailure)
                return Result<Guid>.Failure(workoutResult.Error);

            await _repository.AddAsync(workoutResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(workoutResult.Value.Id);
        }
    }
}
