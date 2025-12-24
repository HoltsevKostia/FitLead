using FitLead.Application.Abstractions.Persistence;
using MediatR;

namespace FitLead.Application.Trainings.Queries.Exercise
{
    public sealed class GetExercisesByTrainerHandler
    : IRequestHandler<GetExercisesByTrainerQuery, IReadOnlyList<ExerciseDto>>
    {
        private readonly IExerciseReadRepository _repository;

        public GetExercisesByTrainerHandler(IExerciseReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ExerciseDto>> Handle(
            GetExercisesByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetByTrainerIdAsync(
                request.TrainerId,
                cancellationToken);
        }
    }
}
