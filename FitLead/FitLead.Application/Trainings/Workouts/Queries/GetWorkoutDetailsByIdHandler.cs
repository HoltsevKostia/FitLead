using FitLead.Application.Abstractions.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed class GetWorkoutDetailsByIdHandler
        : IRequestHandler<GetWorkoutDetailsByIdQuery, WorkoutDetailsDto?>
    {
        private readonly IWorkoutReadRepository _repository;
        public GetWorkoutDetailsByIdHandler(IWorkoutReadRepository workoutReadRepository) {
            _repository = workoutReadRepository;
        }

        public async Task<WorkoutDetailsDto?> Handle(GetWorkoutDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _repository.GetWorkoutDetailsByIdAsync(
                request.WorkoutId,
                request.TrainerId,
                cancellationToken);

            if (dto is null)
                throw new KeyNotFoundException("Workout not found");

            return dto;
        }
    }
}

