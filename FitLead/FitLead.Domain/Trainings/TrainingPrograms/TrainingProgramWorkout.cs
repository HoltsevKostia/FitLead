using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings.TrainingPrograms
{
    public sealed class TrainingProgramWorkout : Entity<Guid>
    {
        public Guid TrainingProgramId { get; private set; }
        public Guid WorkoutId { get; private set; }
        public int Order { get; private set; }

        private TrainingProgramWorkout() { } // EF

        private TrainingProgramWorkout(
            Guid id,
            Guid workoutId,
            int order,
            Guid trainingProgramId)
        {
            Id = id;
            WorkoutId = workoutId;
            Order = order;
            TrainingProgramId = trainingProgramId;
        }

        internal static Result<TrainingProgramWorkout> Create(
            Guid id,
            Guid workoutId,
            int order,
            Guid trainingProgramId)
        {
            if (workoutId == Guid.Empty)
                return Result<TrainingProgramWorkout>.Failure(
                    Error.Validation("training_program.workout.create.workout_id_required", "WorkoutId is required"));

            if (order <= 0)
                return Result<TrainingProgramWorkout>.Failure(
                    Error.Validation("training_program.workout.create.order_positive_required", "Order must be positive"));

            return Result<TrainingProgramWorkout>.Success(
                new TrainingProgramWorkout(id, workoutId, order, trainingProgramId));
        }

        internal Result ChangeOrder(int order)
        {
            if (order <= 0)
                return Result.Failure(
                    Error.Validation("training_program.workout.order.positive_required", "Order must be positive"));

            Order = order;

            return Result.Success();
        }
    }
}
