using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.Domain.Trainings.Exercises;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed record ExerciseDto(
        Guid Id,
        string Name,
        string Description,
        MediaAssetPreviewDto? MediaAsset,
        MuscleGroup? MuscleGroup,
        Equipment? Equipment,
        ExerciseSource Source,
        Guid? CopiedFromExerciseId,
        bool IsEditable
    );
}
