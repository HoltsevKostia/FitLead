using FitLead.Application.Trainings.Queries.Exercise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IExerciseReadRepository
    {
        Task<IReadOnlyList<ExerciseDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
