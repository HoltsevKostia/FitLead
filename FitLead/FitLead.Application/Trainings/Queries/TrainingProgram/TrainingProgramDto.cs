using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Queries.TrainingProgram
{
    public sealed class TrainingProgramDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
    }
}
