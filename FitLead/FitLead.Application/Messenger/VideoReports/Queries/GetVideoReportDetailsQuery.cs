using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.VideoReports.Queries
{
    public sealed record GetVideoReportDetailsQuery(
        Guid ChatId,
        Guid ReportId)
        : IRequest<Result<VideoReportDetailsDto>>;
}
