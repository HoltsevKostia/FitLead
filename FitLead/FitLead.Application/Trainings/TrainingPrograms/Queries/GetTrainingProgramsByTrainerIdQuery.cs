using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed record GetTrainingProgramsByTrainerIdQuery(
        
    ) : IRequest<Result<IReadOnlyList<TrainingProgramDto>>>;
}
