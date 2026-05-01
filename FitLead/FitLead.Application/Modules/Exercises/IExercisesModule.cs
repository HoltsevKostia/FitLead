namespace FitLead.Application.Modules.Exercises
{
    public interface IExercisesModule
    {
        Task<ExerciseModuleDescriptor?> GetByIdAsync(
            Guid exerciseId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Guid exerciseId,
            CancellationToken cancellationToken = default);
    }
}
