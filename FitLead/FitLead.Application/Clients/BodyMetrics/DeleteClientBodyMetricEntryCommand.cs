using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.BodyMetrics
{
    public sealed record DeleteClientBodyMetricEntryCommand(Guid EntryId) : IRequest<Result>;
}
