using FitLead.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Trainings
{
    public sealed class Exercise : Entity<Guid>
    {
        public string Name { get; private set; } = null!;
        public int Repetitions { get; private set; }
        public int Sets { get; private set; }

        private Exercise() { }

        public Exercise(Guid id, string name, int repetitions, int sets)
        {
            if (repetitions <= 0 || sets <= 0)
                throw new ArgumentException("Repetitions and sets must be positive");

            Id = id;
            Name = name;
            Repetitions = repetitions;
            Sets = sets;
        }
    }
}
