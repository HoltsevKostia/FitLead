using FitLead.Domain.Common;
using FitLead.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Trainings
{
    public sealed class WorkoutExercise : Entity<Guid>
    {
        public Guid ExerciseId { get; private set; }
        public int Repetitions { get; private set; }
        public int Sets { get; private set; }
        public int RestSeconds { get; private set; }

        private WorkoutExercise() { } // EF

        internal WorkoutExercise(
            Guid id,
            Guid exerciseId,
            int repetitions,
            int sets,
            int restSeconds)
        {
            if (exerciseId == Guid.Empty)
                throw new ArgumentException("ExerciseId is required");

            if (repetitions <= 0 || sets <= 0)
                throw new ArgumentException("Invalid repetitions or sets");

            if (restSeconds < 0)
                throw new ArgumentException("RestSeconds cannot be negative");

            Id = id;
            ExerciseId = exerciseId;
            Repetitions = repetitions;
            Sets = sets;
            RestSeconds = restSeconds;
        }

        public void Update(
            int repetitions,
            int sets,
            int restSeconds)
        {
            if (repetitions <= 0 || sets <= 0)
                throw new DomainRuleViolationException("workout.exercise.update.invalid_reps_or_sets", "Invalid repetitions or sets");

            if (restSeconds < 0)
                throw new DomainRuleViolationException("workout.exercise.update.rest_seconds_negative", "RestSeconds cannot be negative");

            Repetitions = repetitions;
            Sets = sets;
            RestSeconds = restSeconds;
        }
    }
}
