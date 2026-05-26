using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.BodyMetrics
{
    public sealed record GetClientBodyMetricEntriesQuery
        : IRequest<Result<IReadOnlyList<ClientBodyMetricEntryDto>>>;
}
