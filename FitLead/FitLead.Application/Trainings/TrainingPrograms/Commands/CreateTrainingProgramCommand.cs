using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public record CreateTrainingProgramCommand(
        string Title
    ) : IRequest<Result<Guid>>;
}
