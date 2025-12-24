using FitLead.Domain.Trainings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IExerciseRepository
    {
        Task AddAsync(Exercise exercise, CancellationToken cancellationToken);
        Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
