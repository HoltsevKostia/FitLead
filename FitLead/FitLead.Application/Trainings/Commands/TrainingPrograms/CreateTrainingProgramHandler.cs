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

namespace FitLead.Application.Trainings.Commands.TrainingPrograms
{
    public class CreateTrainingProgramHandler
    : IRequestHandler<CreateTrainingProgramCommand, Result<Guid>>
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTrainingProgramHandler(
            ITrainingProgramRepository programRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _programRepository = programRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateTrainingProgramCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(
                request.TrainerId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure("Trainer not found");

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure("User is not a trainer");

            var program = TrainingProgram.Create(
                request.Title,
                trainer.Id);

            await _programRepository.AddAsync(program, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(program.Id);
        }
    }
}
