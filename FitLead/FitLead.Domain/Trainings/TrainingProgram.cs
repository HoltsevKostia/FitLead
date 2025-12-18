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

        private TrainingProgram() { } // для EF

        private TrainingProgram(Guid id, string title, Trainer trainer)
        {
            Id = id;
            Title = title;
            Trainer = trainer;
        }

        public static TrainingProgram Create(
            string title,
            Trainer trainer)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required", nameof(title));

            if (trainer is null)
                throw new ArgumentNullException(nameof(trainer));

            return new TrainingProgram(
                Guid.NewGuid(),
                title.Trim(),
                trainer);
        }

        public void AddWorkout(Workout workout)
        {
            if (workout is null)
                throw new ArgumentNullException(nameof(workout));

            _workouts.Add(workout);
        }
    }
}
