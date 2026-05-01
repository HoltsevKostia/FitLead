using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Modules.Workouts;

namespace FitLead.Infrastructure.Modules.Workouts
{
    public sealed class WorkoutsModule : IWorkoutsModule
    {
        private readonly IWorkoutRepository _workoutRepository;

        public WorkoutsModule(IWorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }

        public async Task<WorkoutModuleDescriptor?> GetByIdAsync(
            Guid workoutId,
            CancellationToken cancellationToken = default)
        {
            var trainerId = await _workoutRepository.GetTrainerIdAsync(
                workoutId,
                cancellationToken);

            if (!trainerId.HasValue)
                return null;

            return new WorkoutModuleDescriptor(workoutId, trainerId.Value);
        }
    }
}
