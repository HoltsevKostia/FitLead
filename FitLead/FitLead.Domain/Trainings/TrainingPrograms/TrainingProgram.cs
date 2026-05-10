using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings.TrainingPrograms
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

        public static Result<TrainingProgram> Create(string title, Guid trainerId)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result<TrainingProgram>.Failure(
                    Error.Validation("training_program.create.title_required", "Title is required"));

            if (trainerId == Guid.Empty)
                return Result<TrainingProgram>.Failure(
                    Error.Validation("training_program.create.trainer_id_required", "TrainerId is required"));

            return Result<TrainingProgram>.Success(
                new TrainingProgram(Guid.NewGuid(), title.Trim(), trainerId));
        }

        public Result AddWorkout(Guid workoutId)
        {
            if (workoutId == Guid.Empty)
                return Result.Failure(
                    Error.Validation("training_program.workouts.add.workout_id_required", "WorkoutId is required"));

            if (_workouts.Any(x => x.WorkoutId == workoutId))
                return Result.Failure(
                    Error.Conflict("training_program.workouts.add.already_exists", "Workout already added to program"));

            var order = _workouts.Count + 1;

            var entryResult = TrainingProgramWorkout.Create(
                Guid.NewGuid(),
                workoutId,
                order,
                Id);

            if (entryResult.IsFailure)
                return Result.Failure(entryResult.Error);

            _workouts.Add(entryResult.Value);

            return Result.Success();
        }

        public Result RemoveWorkout(Guid workoutId)
        {
            var entry = _workouts.FirstOrDefault(x => x.WorkoutId == workoutId);
            if (entry is null)
                return Result.Failure(
                    Error.NotFound("training_program.workouts.remove.not_found", "Workout not found"));

            _workouts.Remove(entry);

            var order = 1;
            foreach (var w in _workouts.OrderBy(x => x.Order))
            {
                var changeResult = w.ChangeOrder(order++);
                if (changeResult.IsFailure)
                    return changeResult;
            }

            return Result.Success();
        }

        public Result ReorderWorkouts(IReadOnlyList<Guid> orderedWorkoutIds)
        {
            if (orderedWorkoutIds is null || orderedWorkoutIds.Count == 0)
                return Result.Failure(
                    Error.Validation("training_program.workouts.order.required", "Workout order list is required"));

            var existingIds = Workouts.Select(x => x.WorkoutId).ToHashSet();

            if (existingIds.Count != orderedWorkoutIds.Count)
                return Result.Failure(
                    Error.Validation("training_program.workouts.order.invalid_count", "Workout order list must include all workouts from the program"));

            foreach (var id in orderedWorkoutIds)
            {
                if (!existingIds.Contains(id))
                    return Result.Failure(
                        Error.Validation("training_program.workouts.order.contains_unknown_workout", "Workout order list contains workout not in the program"));
            }

            var order = 1;
            foreach (var id in orderedWorkoutIds)
            {
                var map = _workouts.ToDictionary(x => x.WorkoutId);
                var link = map[id];
                var changeResult = link.ChangeOrder(order++);
                if (changeResult.IsFailure)
                    return changeResult;
            }

            return Result.Success();
        }
    }
}
