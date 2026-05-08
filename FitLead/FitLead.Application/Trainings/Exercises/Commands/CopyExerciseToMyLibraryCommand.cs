using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record CopyExerciseToMyLibraryCommand(
        Guid ExerciseId
    ) : IRequest<Result<Guid>>;
}
