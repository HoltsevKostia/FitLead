using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientVideoReportsQuery(Guid ClientId)
        : IRequest<Result<IReadOnlyList<TrainerClientVideoReportDto>>>;
}
