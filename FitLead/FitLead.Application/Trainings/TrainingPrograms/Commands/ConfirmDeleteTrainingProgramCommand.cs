using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed record ConfirmDeleteTrainingProgramCommand(
        Guid ProgramId,
        string Token
    ) : IRequest<Result>;
}
