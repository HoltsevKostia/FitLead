using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class GetTrainingProgramsByTrainerIdHandler
    : IRequestHandler<
        GetTrainingProgramsByTrainerIdQuery,
        Result<IReadOnlyList<TrainingProgramDto>>>
    {
        private readonly IUserContext _user;
        private readonly ITrainingProgramReadRepository _repository;

        public GetTrainingProgramsByTrainerIdHandler(
            IUserContext user,
            ITrainingProgramReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<TrainingProgramDto>>> Handle(
            GetTrainingProgramsByTrainerIdQuery request,
            CancellationToken cancellationToken)
        {
            var programs = await _repository.GetByTrainerIdAsync(
                _user.UserId,
                cancellationToken);
            return Result<IReadOnlyList<TrainingProgramDto>>.Success(programs);
        }
    }
}
