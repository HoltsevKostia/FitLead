using FitLead.Domain.Common;
using FitLead.Domain.Common.Exceptions;
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
        private readonly List<TrainingProgramWorkout> _workouts = new();

        public string Title { get; private set; } = null!;
        public Guid TrainerId { get; private set; }

        public IReadOnlyCollection<TrainingProgramWorkout> Workouts => _workouts.AsReadOnly();

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

        public void AddWorkout(Guid workoutId)
        {
            if (_workouts.Any(x => x.WorkoutId == workoutId))
                throw new InvalidOperationException("Workout already added to program");

            var order = _workouts.Count + 1;

            var entry = new TrainingProgramWorkout(
                Guid.NewGuid(),
                workoutId,
                order,
                Id);

            _workouts.Add(entry);
        }

        public void RemoveWorkout(Guid workoutId)
        {
            var entry = _workouts.FirstOrDefault(x => x.WorkoutId == workoutId);
            if (entry is null)
                throw new InvalidOperationException("Workout not found");

            _workouts.Remove(entry);

            var order = 1;
            foreach (var w in _workouts.OrderBy(x => x.Order))
                w.ChangeOrder(order++);
        }

        public void ReorderWorkouts(IReadOnlyList<Guid> orderedWorkoutIds)
        {
            if (orderedWorkoutIds is null || orderedWorkoutIds.Count == 0)
                throw new DomainRuleViolationException(
                    "training_program.workouts.order.required",
                    "Workout order list is required");

            var existingIds = Workouts.Select(x => x.WorkoutId).ToHashSet();

            if (existingIds.Count != orderedWorkoutIds.Count)
                throw new DomainRuleViolationException(
                    "training_program.workouts.order.invalid_count",
                    "Workout order list must include all workouts from the program");

            foreach (var id in orderedWorkoutIds)
            {
                if (!existingIds.Contains(id))
                    throw new DomainRuleViolationException(
                        "training_program.workouts.order.contains_unknown_workout",
                        "Workout order list contains workout not in the program");
            }

            var order = 1;
            foreach (var id in orderedWorkoutIds)
            {
                var link = Workouts.First(x => x.WorkoutId == id);
                link.ChangeOrder(order++);
            }
        }
    }
}
