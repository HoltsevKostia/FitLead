using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.TrainerVideoReports.Queries
{
    public sealed record GetTrainerPendingVideoReportsQuery
        : IRequest<Result<IReadOnlyList<TrainerPendingVideoReportDto>>>;
}
