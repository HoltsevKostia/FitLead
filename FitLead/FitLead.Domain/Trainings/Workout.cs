using FitLead.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Trainings
{
    public sealed class Workout : Entity<Guid>
    {
        private readonly List<Exercise> _exercises = new();

        public string Name { get; private set; } = null!;

        public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

        private Workout() { }

        public Workout(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public void AddExercise(Exercise exercise)
        {
            _exercises.Add(exercise);
        }
    }
}
