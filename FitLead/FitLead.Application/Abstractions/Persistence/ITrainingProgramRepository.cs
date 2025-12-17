using FitLead.Domain.Trainings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainingProgramRepository
    {
        Task AddAsync(TrainingProgram program, CancellationToken cancellationToken);
    }
}
