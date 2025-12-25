using FitLead.Application.Abstractions.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Queries.Workout
{
    public sealed class GetWorkoutsByTrainerHandler
    : IRequestHandler<GetWorkoutsByTrainerQuery, IReadOnlyList<WorkoutDto>>
    {
        private readonly IWorkoutReadRepository _repository;

        public GetWorkoutsByTrainerHandler(IWorkoutReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<WorkoutDto>> Handle(
            GetWorkoutsByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetByTrainerIdAsync(
                request.TrainerId,
                cancellationToken);
        }
    }
}
