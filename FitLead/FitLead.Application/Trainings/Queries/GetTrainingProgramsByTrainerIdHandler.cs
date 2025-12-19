using FitLead.Application.Abstractions.Persistence;
using MediatR;

namespace FitLead.Application.Trainings.Queries
{
    public sealed class GetTrainingProgramsByTrainerIdHandler
    : IRequestHandler<
        GetTrainingProgramsByTrainerIdQuery,
        IReadOnlyList<TrainingProgramDto>>
    {
        private readonly ITrainingProgramReadRepository _repository;

        public GetTrainingProgramsByTrainerIdHandler(
            ITrainingProgramReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<TrainingProgramDto>> Handle(
            GetTrainingProgramsByTrainerIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetByTrainerIdAsync(
                request.TrainerId,
                cancellationToken);
        }
    }
}
