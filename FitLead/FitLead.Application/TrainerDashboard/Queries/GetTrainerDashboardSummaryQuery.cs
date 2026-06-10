using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.TrainerDashboard.Queries
{
    public sealed record GetTrainerDashboardSummaryQuery
        : IRequest<Result<TrainerDashboardSummaryDto>>;
}
