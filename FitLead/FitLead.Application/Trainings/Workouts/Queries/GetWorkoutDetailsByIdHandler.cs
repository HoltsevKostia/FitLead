using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
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
        private readonly IUserContext _user;
        private readonly IWorkoutReadRepository _repository;
        public GetWorkoutDetailsByIdHandler(IUserContext user, IWorkoutReadRepository workoutReadRepository) {
            _user = user;
            _repository = workoutReadRepository;
        }

        public async Task<WorkoutDetailsDto?> Handle(GetWorkoutDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _repository.GetWorkoutDetailsByIdAsync(
                request.WorkoutId,
                _user.UserId,
                cancellationToken);

            if (dto is null)
                throw new KeyNotFoundException("Workout not found");

            return dto;
        }
    }
}

