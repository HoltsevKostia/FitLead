using FitLead.Application.Trainings.Queries.Workout;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Queries.TrainingProgram
{
    public sealed record GetWorkoutsByProgramIdQuery(
        Guid ProgramId,
        Guid TrainerId
    ) : IRequest<IReadOnlyList<WorkoutDto>>;
}
