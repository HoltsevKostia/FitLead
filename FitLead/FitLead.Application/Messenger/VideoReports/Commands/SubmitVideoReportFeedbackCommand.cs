using FitLead.Application.Messenger.VideoReports.Queries;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.VideoReports.Commands
{
    public sealed record SubmitVideoReportFeedbackCommand(
        Guid ChatId,
        Guid ReportId,
        string Text)
        : IRequest<Result<VideoReportDetailsDto>>;
}
