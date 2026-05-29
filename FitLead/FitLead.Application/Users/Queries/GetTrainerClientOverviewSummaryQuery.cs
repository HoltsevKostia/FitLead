using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientOverviewSummaryQuery(Guid ClientId)
        : IRequest<Result<TrainerClientOverviewSummaryDto>>;
}
