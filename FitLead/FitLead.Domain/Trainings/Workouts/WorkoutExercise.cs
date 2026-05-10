using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings.Workouts
{
    public sealed class WorkoutExercise : Entity<Guid>
    {
        public Guid ExerciseId { get; private set; }
        public int Repetitions { get; private set; }
        public int Sets { get; private set; }
        public int RestSeconds { get; private set; }

        private WorkoutExercise() { } // EF

        private WorkoutExercise(
            Guid id,
            Guid exerciseId,
            int repetitions,
            int sets,
            int restSeconds)
        {
            Id = id;
            ExerciseId = exerciseId;
            Repetitions = repetitions;
            Sets = sets;
            RestSeconds = restSeconds;
        }

        internal static Result<WorkoutExercise> Create(
            Guid id,
            Guid exerciseId,
            int repetitions,
            int sets,
            int restSeconds)
        {
            if (exerciseId == Guid.Empty)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.exercise_id_required", "ExerciseId is required"));

            if (repetitions <= 0 || sets <= 0)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.invalid_reps_or_sets", "Invalid repetitions or sets"));

            if (restSeconds < 0)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.rest_seconds_negative", "RestSeconds cannot be negative"));

            return Result<WorkoutExercise>.Success(
                new WorkoutExercise(id, exerciseId, repetitions, sets, restSeconds));
        }

        public Result Update(
            int repetitions,
            int sets,
            int restSeconds)
        {
            if (repetitions <= 0 || sets <= 0)
                return Result.Failure(
                    Error.Validation("workout.exercise.update.invalid_reps_or_sets", "Invalid repetitions or sets"));

            if (restSeconds < 0)
                return Result.Failure(
                    Error.Validation("workout.exercise.update.rest_seconds_negative", "RestSeconds cannot be negative"));

            Repetitions = repetitions;
            Sets = sets;
            RestSeconds = restSeconds;

            return Result.Success();
        }
    }
}
