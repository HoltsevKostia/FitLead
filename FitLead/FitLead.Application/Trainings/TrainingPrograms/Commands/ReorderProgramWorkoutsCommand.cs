using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed record ReorderProgramWorkoutsCommand(
        Guid ProgramId,
        IReadOnlyList<Guid> OrderedWorkoutIds
    ) : IRequest<Result>;
}
