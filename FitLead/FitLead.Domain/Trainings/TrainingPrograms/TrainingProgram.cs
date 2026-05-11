using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings.TrainingPrograms
{
    public sealed class TrainingProgram : AggregateRoot<Guid>
    {
        public const int MaxWeeksCount = 24;
        public const int MaxDaysPerWeek = 7;

        private readonly List<TrainingProgramWorkout> _workouts = new();

        public string Title { get; private set; } = null!;
        public Guid TrainerId { get; private set; }
        public int WeeksCount { get; private set; }
        public int DaysPerWeek { get; private set; }

        public IReadOnlyCollection<TrainingProgramWorkout> Workouts => _workouts.AsReadOnly();

        private TrainingProgram() { } // EF

        private TrainingProgram(Guid id, string title, Guid trainerId, int weeksCount, int daysPerWeek)
        {
            Id = id;
            Title = title;
            TrainerId = trainerId;
            WeeksCount = weeksCount;
            DaysPerWeek = daysPerWeek;
        }

        public static Result<TrainingProgram> Create(
            Guid trainerId,
            string title,
            int weeksCount,
            int daysPerWeek)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result<TrainingProgram>.Failure(
                    Error.Validation("training_program.create.title_required", "Title is required"));

            if (trainerId == Guid.Empty)
                return Result<TrainingProgram>.Failure(
                    Error.Validation("training_program.create.trainer_id_required", "TrainerId is required"));

            if (weeksCount is < 1 or > MaxWeeksCount)
                return Result<TrainingProgram>.Failure(
                    Error.Validation("training_program.create.weeks_count_out_of_range", $"WeeksCount must be between 1 and {MaxWeeksCount}"));

            if (daysPerWeek is < 1 or > MaxDaysPerWeek)
                return Result<TrainingProgram>.Failure(
                    Error.Validation("training_program.create.days_per_week_out_of_range", $"DaysPerWeek must be between 1 and {MaxDaysPerWeek}"));

            return Result<TrainingProgram>.Success(
                new TrainingProgram(Guid.NewGuid(), title.Trim(), trainerId, weeksCount, daysPerWeek));
        }

        public Result AddWorkout(Guid workoutId, int weekNumber, int dayNumber)
        {
            if (workoutId == Guid.Empty)
                return Result.Failure(
                    Error.Validation("training_program.workouts.add.workout_id_required", "WorkoutId is required"));

            var slotValidationResult = ValidateProgramSlot(weekNumber, dayNumber);
            if (slotValidationResult.IsFailure)
                return slotValidationResult;

            var orderInDay = _workouts
                .Where(x => x.WeekNumber == weekNumber && x.DayNumber == dayNumber)
                .Select(x => x.OrderInDay)
                .DefaultIfEmpty(0)
                .Max() + 1;

            if (_workouts.Any(x =>
                    x.WeekNumber == weekNumber &&
                    x.DayNumber == dayNumber &&
                    x.OrderInDay == orderInDay))
            {
                return Result.Failure(
                    Error.Conflict("training_program.workouts.add.slot_occupied", "Program day slot is already occupied"));
            }

            var entryResult = TrainingProgramWorkout.Create(
                Guid.NewGuid(),
                workoutId,
                weekNumber,
                dayNumber,
                orderInDay,
                Id);

            if (entryResult.IsFailure)
                return Result.Failure(entryResult.Error);

            _workouts.Add(entryResult.Value);

            return Result.Success();
        }

        public Result RemoveWorkoutEntry(Guid trainingProgramWorkoutId)
        {
            if (trainingProgramWorkoutId == Guid.Empty)
                return Result.Failure(
                    Error.Validation("training_program.workouts.remove.entry_id_required", "TrainingProgramWorkoutId is required"));

            var entry = _workouts.FirstOrDefault(x => x.Id == trainingProgramWorkoutId);
            if (entry is null)
                return Result.Failure(
                    Error.NotFound("training_program.workouts.remove.not_found", "Program workout entry not found"));

            _workouts.Remove(entry);

            return ReorderDay(entry.WeekNumber, entry.DayNumber);
        }

        public Result MoveWorkoutEntry(
            Guid trainingProgramWorkoutId,
            int targetWeekNumber,
            int targetDayNumber,
            int targetOrderInDay)
        {
            if (trainingProgramWorkoutId == Guid.Empty)
                return Result.Failure(
                    Error.Validation("training_program.workouts.move.entry_id_required", "TrainingProgramWorkoutId is required"));

            var entry = _workouts.FirstOrDefault(x => x.Id == trainingProgramWorkoutId);
            if (entry is null)
                return Result.Failure(
                    Error.NotFound("training_program.workouts.move.not_found", "Program workout entry not found"));

            var slotValidationResult = ValidateProgramSlot(targetWeekNumber, targetDayNumber);
            if (slotValidationResult.IsFailure)
                return slotValidationResult;

            if (targetOrderInDay <= 0)
                return Result.Failure(
                    Error.Validation("training_program.workouts.move.order_in_day_positive_required", "OrderInDay must be positive"));

            var sourceWeekNumber = entry.WeekNumber;
            var sourceDayNumber = entry.DayNumber;
            var targetEntries = _workouts
                .Where(x =>
                    x.Id != entry.Id &&
                    x.WeekNumber == targetWeekNumber &&
                    x.DayNumber == targetDayNumber)
                .OrderBy(x => x.OrderInDay)
                .ToList();
            var targetEntriesCount = targetEntries.Count;
            var normalizedTargetOrder = Math.Min(targetOrderInDay, targetEntriesCount + 1);
            var isSameDay = sourceWeekNumber == targetWeekNumber && sourceDayNumber == targetDayNumber;

            if (!isSameDay)
            {
                var reorderSourceResult = ReorderDay(sourceWeekNumber, sourceDayNumber, entry.Id);
                if (reorderSourceResult.IsFailure)
                    return reorderSourceResult;
            }

            targetEntries.Insert(normalizedTargetOrder - 1, entry);

            var order = 1;
            foreach (var targetEntry in targetEntries)
            {
                var moveResult = targetEntry.MoveTo(targetWeekNumber, targetDayNumber, order++);
                if (moveResult.IsFailure)
                    return moveResult;
            }

            return Result.Success();
        }

        public Result ReorderDay(
            int weekNumber,
            int dayNumber,
            IReadOnlyList<Guid> orderedEntryIds)
        {
            var slotValidationResult = ValidateProgramSlot(weekNumber, dayNumber);
            if (slotValidationResult.IsFailure)
                return slotValidationResult;

            if (orderedEntryIds is null || orderedEntryIds.Count == 0)
                return Result.Failure(
                    Error.Validation("training_program.workouts.reorder_day.required", "Program workout entry order list is required"));

            if (orderedEntryIds.Distinct().Count() != orderedEntryIds.Count)
                return Result.Failure(
                    Error.Validation("training_program.workouts.reorder_day.contains_duplicates", "Program workout entry order list must not contain duplicates"));

            var entriesInDay = _workouts
                .Where(x => x.WeekNumber == weekNumber && x.DayNumber == dayNumber)
                .ToList();

            var existingEntryIds = entriesInDay.Select(x => x.Id).ToHashSet();

            if (existingEntryIds.Count != orderedEntryIds.Count)
                return Result.Failure(
                    Error.Validation("training_program.workouts.reorder_day.invalid_count", "Program workout entry order list must include all entries from the day"));

            if (orderedEntryIds.Any(id => !existingEntryIds.Contains(id)))
                return Result.Failure(
                    Error.Validation("training_program.workouts.reorder_day.contains_unknown_entry", "Program workout entry order list contains entry not in the day"));

            var order = 1;
            var entriesById = entriesInDay.ToDictionary(x => x.Id);
            foreach (var id in orderedEntryIds)
            {
                var link = entriesById[id];
                var changeResult = link.ChangeOrderInDay(order++);
                if (changeResult.IsFailure)
                    return changeResult;
            }

            return Result.Success();
        }

        private Result ValidateProgramSlot(int weekNumber, int dayNumber)
        {
            if (weekNumber < 1 || weekNumber > WeeksCount)
                return Result.Failure(
                    Error.Validation("training_program.workouts.week_number_out_of_range", "WeekNumber must be within program weeks"));

            if (dayNumber < 1 || dayNumber > DaysPerWeek)
                return Result.Failure(
                    Error.Validation("training_program.workouts.day_number_out_of_range", "DayNumber must be within program days per week"));

            return Result.Success();
        }

        private Result ReorderDay(int weekNumber, int dayNumber, Guid? excludedEntryId = null)
        {
            var order = 1;
            foreach (var w in _workouts
                         .Where(x =>
                             x.WeekNumber == weekNumber &&
                             x.DayNumber == dayNumber &&
                             x.Id != excludedEntryId)
                         .OrderBy(x => x.OrderInDay))
            {
                var changeResult = w.ChangeOrderInDay(order++);
                if (changeResult.IsFailure)
                    return changeResult;
            }

            return Result.Success();
        }
    }
}
