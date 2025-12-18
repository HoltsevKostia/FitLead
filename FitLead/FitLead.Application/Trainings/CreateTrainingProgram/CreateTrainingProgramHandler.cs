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
        private readonly ITrainingProgramRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTrainingProgramHandler(ITrainingProgramRepository repository, 
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateTrainingProgramCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<Guid>.Failure("Training program title is required");

            var program = TrainingProgram.Create(
            request.Title,
            request.TrainerId);

            await _repository.AddAsync(program, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(program.Id);
        }
    }
}
