using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

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

        public static Result<Workout> Create(string name, Guid trainerId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Workout>.Failure(
                    Error.Validation("workout.create.name_required", "Workout name is required"));

            if (trainerId == Guid.Empty)
                return Result<Workout>.Failure(
                    Error.Validation("workout.create.trainer_id_required", "TrainerId is required"));

            return Result<Workout>.Success(
                new Workout(Guid.NewGuid(), name.Trim(), trainerId));
        }

        public Result Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(
                    Error.Validation("workout.rename.name_required", "Workout name is required"));

            Name = name.Trim();

            return Result.Success();
        }

        public Result<Guid> AddExercise(
            Guid exerciseId,
            int repetitions,
            int sets,
            int restSeconds)
        {
            var entryId = Guid.NewGuid();

            var entryResult = WorkoutExercise.Create(
                entryId,
                exerciseId,
                repetitions,
                sets,
                restSeconds);

            if (entryResult.IsFailure)
                return Result<Guid>.Failure(entryResult.Error);

            _exercises.Add(entryResult.Value);
            
            return Result<Guid>.Success(entryId);
        }

        public Result RemoveExercise(Guid workoutExerciseId)
        {
            var entry = _exercises.FirstOrDefault(x => x.Id == workoutExerciseId);
            if (entry is null)
                return Result.Failure(
                    Error.Validation("workout.exercise.remove.not_found", "Exercise not found in workout"));

            _exercises.Remove(entry);

            return Result.Success();
        }

        public Result UpdateExercise(Guid workoutExerciseId, int repetitions, int sets, int restSeconds)
        {
            var entry = _exercises.FirstOrDefault(x => x.Id == workoutExerciseId);
            if (entry is null)
                return Result.Failure(
                    Error.Validation("workout.exercise.update.not_found", "Exercise not found in workout"));

            var updateResult = entry.Update(repetitions, sets, restSeconds);
            if (updateResult.IsFailure)
                return updateResult;

            return Result.Success();
        }
    }
}
