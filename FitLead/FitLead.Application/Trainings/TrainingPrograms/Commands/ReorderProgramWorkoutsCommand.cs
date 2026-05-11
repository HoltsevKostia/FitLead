using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed record ReorderProgramWorkoutsCommand(
        Guid ProgramId,
        int WeekNumber,
        int DayNumber,
        IReadOnlyList<Guid> OrderedEntryIds
    ) : IRequest<Result>;
}
