using FitLead.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Trainings
{
    public sealed class TrainingProgramWorkout : Entity<Guid>
    {
        public Guid WorkoutId { get; private set; }
        public int Order { get; private set; }

        private TrainingProgramWorkout() { } // EF

        internal TrainingProgramWorkout(Guid id, Guid workoutId, int order)
        {
            if (workoutId == Guid.Empty)
                throw new ArgumentException("WorkoutId is required");

            if (order < 0)
                throw new ArgumentException("Order must be non-negative");

            Id = id;
            WorkoutId = workoutId;
            Order = order;
        }

        public void ChangeOrder(int order)
        {
            if (order < 0)
                throw new ArgumentException("Order must be non-negative");

            Order = order;
        }
    }
}
