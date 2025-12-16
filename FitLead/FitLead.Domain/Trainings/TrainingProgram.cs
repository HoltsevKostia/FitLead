using FitLead.Domain.Common;
using FitLead.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Trainings
{
    public sealed class TrainingProgram : AggregateRoot<Guid>
    {
        private readonly List<Workout> _workouts = new();

        public string Title { get; private set; } = null!;
        public Trainer Trainer { get; private set; } = null!;

        public IReadOnlyCollection<Workout> Workouts => _workouts.AsReadOnly();

        private TrainingProgram() { }

        public TrainingProgram(Guid id, string title, Trainer trainer)
        {
            Id = id;
            Title = title;
            Trainer = trainer;
        }

        public void AddWorkout(Workout workout)
        {
            _workouts.Add(workout);
        }
    }
}
