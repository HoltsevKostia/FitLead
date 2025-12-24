using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Queries.Exercise
{
    public sealed record ExerciseDto(
    Guid Id,
    string Name,
    string Description,
    string? MediaUrl
);
}
