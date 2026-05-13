using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientsOverviewQuery(

    ) : IRequest<Result<IReadOnlyList<TrainerClientOverviewDto>>>;
}
