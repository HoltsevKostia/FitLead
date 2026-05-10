using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings.Workouts
{
    public sealed class WorkoutExercise : Entity<Guid>
    {
        public const int MaxTrainerNoteLength = 1000;

        public Guid ExerciseId { get; private set; }
        public Guid WorkoutId { get; private set; }
        public int Order { get; private set; }
        public int Repetitions { get; private set; }
        public int Sets { get; private set; }
        public decimal? LoadKg { get; private set; }
        public int RestSeconds { get; private set; }
        public string? TrainerNote { get; private set; }

        private WorkoutExercise() { } // EF

        private WorkoutExercise(
            Guid id,
            Guid workoutId,
            Guid exerciseId,
            int order,
            int repetitions,
            int sets,
            decimal? loadKg,
            int restSeconds,
            string? trainerNote)
        {
            Id = id;
            WorkoutId = workoutId;
            ExerciseId = exerciseId;
            Order = order;
            Repetitions = repetitions;
            Sets = sets;
            LoadKg = loadKg;
            RestSeconds = restSeconds;
            TrainerNote = trainerNote;
        }

        internal static Result<WorkoutExercise> Create(
            Guid id,
            Guid workoutId,
            Guid exerciseId,
            int order,
            int repetitions,
            int sets,
            decimal? loadKg,
            int restSeconds,
            string? trainerNote)
        {
            if (workoutId == Guid.Empty)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.workout_id_required", "WorkoutId is required"));

            if (exerciseId == Guid.Empty)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.exercise_id_required", "ExerciseId is required"));

            if (order <= 0)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.order_invalid", "Order must be greater than zero"));

            if (repetitions <= 0 || sets <= 0)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.invalid_reps_or_sets", "Invalid repetitions or sets"));

            if (loadKg < 0)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.load_kg_negative", "LoadKg cannot be negative"));

            if (restSeconds < 0)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.rest_seconds_negative", "RestSeconds cannot be negative"));

            var normalizedTrainerNote = NormalizeTrainerNote(trainerNote);
            if (normalizedTrainerNote?.Length > MaxTrainerNoteLength)
                return Result<WorkoutExercise>.Failure(
                    Error.Validation("workout.exercise.create.trainer_note_too_long", $"TrainerNote cannot exceed {MaxTrainerNoteLength} characters"));

            return Result<WorkoutExercise>.Success(
                new WorkoutExercise(
                    id,
                    workoutId,
                    exerciseId,
                    order,
                    repetitions,
                    sets,
                    loadKg,
                    restSeconds,
                    normalizedTrainerNote));
        }

        public Result Update(
            int repetitions,
            int sets,
            decimal? loadKg,
            int restSeconds,
            string? trainerNote)
        {
            if (repetitions <= 0 || sets <= 0)
                return Result.Failure(
                    Error.Validation("workout.exercise.update.invalid_reps_or_sets", "Invalid repetitions or sets"));

            if (loadKg < 0)
                return Result.Failure(
                    Error.Validation("workout.exercise.update.load_kg_negative", "LoadKg cannot be negative"));

            if (restSeconds < 0)
                return Result.Failure(
                    Error.Validation("workout.exercise.update.rest_seconds_negative", "RestSeconds cannot be negative"));

            var normalizedTrainerNote = NormalizeTrainerNote(trainerNote);
            if (normalizedTrainerNote?.Length > MaxTrainerNoteLength)
                return Result.Failure(
                    Error.Validation("workout.exercise.update.trainer_note_too_long", $"TrainerNote cannot exceed {MaxTrainerNoteLength} characters"));

            Repetitions = repetitions;
            Sets = sets;
            LoadKg = loadKg;
            RestSeconds = restSeconds;
            TrainerNote = normalizedTrainerNote;

            return Result.Success();
        }

        private static string? NormalizeTrainerNote(string? trainerNote)
        {
            if (string.IsNullOrWhiteSpace(trainerNote))
                return null;

            return trainerNote.Trim();
        }
    }
}
