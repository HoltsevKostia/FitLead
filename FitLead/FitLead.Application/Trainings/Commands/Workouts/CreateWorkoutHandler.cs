using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using MediatR;


namespace FitLead.Application.Trainings.Commands.Workouts
{
    public sealed class CreateWorkoutHandler
    : IRequestHandler<CreateWorkoutCommand, Result<Guid>>
    {
        private readonly IWorkoutRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWorkoutHandler(
            IWorkoutRepository repository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(
                request.TrainerId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure("Trainer not found");

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure("User is not a trainer");

            var workout = Workout.Create(
                request.Name,
                request.TrainerId);

            await _repository.AddAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(workout.Id);
        }
    }
}
