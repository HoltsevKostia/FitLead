using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientWorkoutLogsQuery(Guid ClientId)
        : IRequest<Result<IReadOnlyList<TrainerClientWorkoutLogDto>>>;
}
