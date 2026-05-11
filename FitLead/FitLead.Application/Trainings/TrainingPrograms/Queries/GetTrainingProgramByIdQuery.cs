using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed record GetTrainingProgramByIdQuery(
        Guid ProgramId
    ) : IRequest<Result<TrainingProgramDto>>;
}
