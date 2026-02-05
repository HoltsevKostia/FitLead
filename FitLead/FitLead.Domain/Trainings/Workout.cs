using FitLead.Domain.Common;
using FitLead.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Trainings
{
    public sealed class Workout : AggregateRoot<Guid>
    {
        private readonly List<WorkoutExercise> _exercises = new();

        public string Name { get; private set; } = null!;
        public Guid TrainerId { get; private set; }

        public IReadOnlyCollection<WorkoutExercise> Exercises => _exercises.AsReadOnly();

        private Workout() { } // EF

        private Workout(Guid id, string name, Guid trainerId)
        {
            Id = id;
            Name = name;
            TrainerId = trainerId;
        }

        public static Workout Create(string name, Guid trainerId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Workout name is required");

            if (trainerId == Guid.Empty)
                throw new ArgumentException("TrainerId is required");

            return new Workout(
                Guid.NewGuid(),
                name.Trim(),
                trainerId);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Workout name is required");

            Name = name.Trim();
        }

        public Guid AddExercise(
            Guid exerciseId,
            int repetitions,
            int sets,
            int restSeconds)
        {
            var entryId = Guid.NewGuid();

            var entry = new WorkoutExercise(
                entryId,
                exerciseId,
                repetitions,
                sets,
                restSeconds);

            _exercises.Add(entry);
            
            return entryId;
        }

        public void RemoveExercise(Guid workoutExerciseId)
        {
            var entry = _exercises.FirstOrDefault(x => x.Id == workoutExerciseId);
            if (entry is null)
                throw new InvalidOperationException("Exercise not found in workout");

            _exercises.Remove(entry);
        }

        public void UpdateExercise(Guid workoutExerciseId, int repetitions, int sets, int restSeconds)
        {
            var entry = _exercises.FirstOrDefault(x => x.Id == workoutExerciseId);
            if (entry is null)
                throw new DomainRuleViolationException("workout.exercise.update.not_found", "Exercise not found in workout");

            entry.Update(repetitions, sets, restSeconds);
        }
    }
}
