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
        public Guid TrainerId { get; private set; }

        public IReadOnlyCollection<Workout> Workouts => _workouts.AsReadOnly();

        private TrainingProgram() { } // EF

        private TrainingProgram(Guid id, string title, Guid trainerId)
        {
            Id = id;
            Title = title;
            TrainerId = trainerId;
        }

        public static TrainingProgram Create(string title, Guid trainerId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (trainerId == Guid.Empty)
                throw new ArgumentException("TrainerId is required");

            return new TrainingProgram(
                Guid.NewGuid(),
                title.Trim(),
                trainerId);
        }

        public void AddWorkout(Workout workout)
        {
            if (workout is null)
                throw new ArgumentNullException(nameof(workout));

            _workouts.Add(workout);
        }
    }
}
