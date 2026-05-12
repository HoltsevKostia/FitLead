using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class GetTrainingProgramByIdHandler
        : IRequestHandler<GetTrainingProgramByIdQuery, Result<TrainingProgramDto>>
    {
        private readonly IUserContext _user;
        private readonly ITrainingProgramReadRepository _repository;

        public GetTrainingProgramByIdHandler(
            IUserContext user,
            ITrainingProgramReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<Result<TrainingProgramDto>> Handle(
            GetTrainingProgramByIdQuery request,
            CancellationToken cancellationToken)
        {
            var program = await _repository.GetByIdAsync(
                request.ProgramId,
                _user.UserId,
                cancellationToken);

            if (program is null)
                return Result<TrainingProgramDto>.Failure(
                    Error.NotFound("training_program.not_found", "Training program not found"));

            return Result<TrainingProgramDto>.Success(program);
        }
    }
}
