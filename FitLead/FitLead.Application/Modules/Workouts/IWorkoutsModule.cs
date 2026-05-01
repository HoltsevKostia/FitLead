namespace FitLead.Application.Modules.Workouts
{
    public interface IWorkoutsModule
    {
        Task<WorkoutModuleDescriptor?> GetByIdAsync(
            Guid workoutId,
            CancellationToken cancellationToken = default);
    }
}
