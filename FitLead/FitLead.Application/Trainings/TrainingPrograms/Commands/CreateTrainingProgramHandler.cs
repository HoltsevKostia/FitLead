using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Common.Results;
using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public class CreateTrainingProgramHandler
    : IRequestHandler<CreateTrainingProgramCommand, Result<Guid>>
    {
        private readonly IUserContext _user;
        private readonly ITrainingProgramRepository _programRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTrainingProgramHandler(
            IUserContext user,
            ITrainingProgramRepository programRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _programRepository = programRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateTrainingProgramCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(
                _user.UserId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure(Error.NotFound("trainer.not_found", "Trainer not found"));

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure(Error.Forbidden("trainer.required", "User is not a trainer"));

            var program = TrainingProgram.Create(
                request.Title,
                _user.UserId);

            await _programRepository.AddAsync(program, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(program.Id);
        }
    }
}
