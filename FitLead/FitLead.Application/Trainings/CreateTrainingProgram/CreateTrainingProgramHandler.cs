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

namespace FitLead.Application.Trainings.CreateTrainingProgram
{
    public class CreateTrainingProgramHandler
    : IRequestHandler<CreateTrainingProgramCommand, Result<Guid>>
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly ITrainerRepository _trainerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTrainingProgramHandler(
            ITrainingProgramRepository programRepository,
            ITrainerRepository trainerRepository,
            IUnitOfWork unitOfWork)
        {
            _programRepository = programRepository;
            _trainerRepository = trainerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateTrainingProgramCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _trainerRepository.GetByIdAsync(
                request.TrainerId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure("Trainer not found");

            var program = TrainingProgram.Create(
                request.Title,
                trainer);

            await _programRepository.AddAsync(program, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(program.Id);
        }
    }
}
