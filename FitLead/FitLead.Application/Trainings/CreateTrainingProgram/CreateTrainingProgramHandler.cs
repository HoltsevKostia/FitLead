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

        public CreateTrainingProgramHandler(ITrainingProgramRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(
            CreateTrainingProgramCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Title))
                return Result<Guid>.Failure("Training program title is required");

            var trainer = new Trainer(
                command.TrainerId,
                "trainer@mail.com",
                "Trainer Name"
            );

            var program = new TrainingProgram(
                Guid.NewGuid(),
                command.Title,
                trainer
            );

            await _repository.AddAsync(program, cancellationToken);

            return Result<Guid>.Success(program.Id);
        }
    }
}
