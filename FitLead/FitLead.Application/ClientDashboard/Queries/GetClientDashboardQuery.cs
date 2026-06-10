using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.ClientDashboard.Queries
{
    public sealed record GetClientDashboardQuery
        : IRequest<Result<ClientDashboardDto>>;
}
