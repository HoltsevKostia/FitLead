using FitLead.Domain.Trainings.Exercises;

namespace FitLead.Application.Modules.Exercises
{
    public sealed record ExerciseModuleDescriptor(
        Guid Id,
        Guid? OwnerTrainerId,
        ExerciseSource Source);
}
