using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using MediatR;


namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class CreateWorkoutHandler
    : IRequestHandler<CreateWorkoutCommand, Result<Guid>>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWorkoutHandler(
            IUserContext user,
            IWorkoutRepository repository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _repository = repository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(
                _user.UserId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure(Error.NotFound("trainer.not_found", "Trainer not found"));

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure(Error.Forbidden("trainer.required", "User is not a trainer"));

            var workout = Workout.Create(
                request.Name,
                _user.UserId);

            await _repository.AddAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(workout.Id);
        }
    }
}
