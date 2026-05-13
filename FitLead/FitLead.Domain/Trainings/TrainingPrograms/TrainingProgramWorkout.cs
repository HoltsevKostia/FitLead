using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings.TrainingPrograms
{
    public sealed class TrainingProgramWorkout : Entity<Guid>
    {
        public Guid TrainingProgramId { get; private set; }
        public Guid WorkoutId { get; private set; }
        public int WeekNumber { get; private set; }
        public int DayNumber { get; private set; }
        public int OrderInDay { get; private set; }

        private TrainingProgramWorkout() { } // EF

        private TrainingProgramWorkout(
            Guid id,
            Guid workoutId,
            int weekNumber,
            int dayNumber,
            int orderInDay,
            Guid trainingProgramId)
        {
            Id = id;
            WorkoutId = workoutId;
            WeekNumber = weekNumber;
            DayNumber = dayNumber;
            OrderInDay = orderInDay;
            TrainingProgramId = trainingProgramId;
        }

        internal static Result<TrainingProgramWorkout> Create(
            Guid id,
            Guid workoutId,
            int weekNumber,
            int dayNumber,
            int orderInDay,
            Guid trainingProgramId)
        {
            if (trainingProgramId == Guid.Empty)
                return Result<TrainingProgramWorkout>.Failure(
                    Error.Validation("training_program.workout.create.training_program_id_required", "TrainingProgramId is required"));

            if (workoutId == Guid.Empty)
                return Result<TrainingProgramWorkout>.Failure(
                    Error.Validation("training_program.workout.create.workout_id_required", "WorkoutId is required"));

            if (weekNumber <= 0)
                return Result<TrainingProgramWorkout>.Failure(
                    Error.Validation("training_program.workout.create.week_number_positive_required", "WeekNumber must be positive"));

            if (dayNumber <= 0)
                return Result<TrainingProgramWorkout>.Failure(
                    Error.Validation("training_program.workout.create.day_number_positive_required", "DayNumber must be positive"));

            if (orderInDay <= 0)
                return Result<TrainingProgramWorkout>.Failure(
                    Error.Validation("training_program.workout.create.order_in_day_positive_required", "OrderInDay must be positive"));

            return Result<TrainingProgramWorkout>.Success(
                new TrainingProgramWorkout(id, workoutId, weekNumber, dayNumber, orderInDay, trainingProgramId));
        }

        internal Result ChangeOrderInDay(int orderInDay)
        {
            if (orderInDay <= 0)
                return Result.Failure(
                    Error.Validation("training_program.workout.order_in_day.positive_required", "OrderInDay must be positive"));

            OrderInDay = orderInDay;

            return Result.Success();
        }

        internal Result MoveTo(int weekNumber, int dayNumber, int orderInDay)
        {
            if (weekNumber <= 0)
                return Result.Failure(
                    Error.Validation("training_program.workout.move.week_number_positive_required", "WeekNumber must be positive"));

            if (dayNumber <= 0)
                return Result.Failure(
                    Error.Validation("training_program.workout.move.day_number_positive_required", "DayNumber must be positive"));

            if (orderInDay <= 0)
                return Result.Failure(
                    Error.Validation("training_program.workout.move.order_in_day_positive_required", "OrderInDay must be positive"));

            WeekNumber = weekNumber;
            DayNumber = dayNumber;
            OrderInDay = orderInDay;

            return Result.Success();
        }
    }
}
