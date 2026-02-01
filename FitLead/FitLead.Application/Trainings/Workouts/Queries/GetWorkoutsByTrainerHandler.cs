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
    public sealed class GetWorkoutsByTrainerHandler
    : IRequestHandler<GetWorkoutsByTrainerQuery, IReadOnlyList<WorkoutDto>>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutReadRepository _repository;

        public GetWorkoutsByTrainerHandler(IUserContext user, IWorkoutReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<IReadOnlyList<WorkoutDto>> Handle(
            GetWorkoutsByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetByTrainerIdAsync(
                _user.UserId,
                cancellationToken);
        }
    }
}
