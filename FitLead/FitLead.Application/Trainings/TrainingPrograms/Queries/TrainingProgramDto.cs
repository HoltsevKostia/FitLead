using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class TrainingProgramDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
    }
}
