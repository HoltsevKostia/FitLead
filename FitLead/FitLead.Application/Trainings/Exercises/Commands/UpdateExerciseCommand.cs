using FitLead.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record UpdateExerciseCommand(
        Guid ExerciseId,
        Guid TrainerId,
        string Name,
        string Description,
        string? MediaUrl
    ) : IRequest<Result>;
}
