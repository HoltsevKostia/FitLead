using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.VideoReports.Queries
{
    public sealed class GetVideoReportDetailsHandler
        : IRequestHandler<GetVideoReportDetailsQuery, Result<VideoReportDetailsDto>>
    {
        private readonly IChatLoader _chatLoader;
        private readonly IVideoReportReadRepository _videoReportReadRepository;

        public GetVideoReportDetailsHandler(
            IChatLoader chatLoader,
            IVideoReportReadRepository videoReportReadRepository)
        {
            _chatLoader = chatLoader;
            _videoReportReadRepository = videoReportReadRepository;
        }

        public async Task<Result<VideoReportDetailsDto>> Handle(
            GetVideoReportDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var chatResult = await _chatLoader.GetAccessibleOrNotFoundAsync(
                request.ChatId,
                cancellationToken);
            if (chatResult.IsFailure)
            {
                return Result<VideoReportDetailsDto>.Failure(chatResult.Error);
            }

            var details = await _videoReportReadRepository.GetDetailsAsync(
                request.ChatId,
                request.ReportId,
                cancellationToken);
            if (details is null)
            {
                return Result<VideoReportDetailsDto>.Failure(
                    Error.NotFound("video_report.not_found", "Video report not found"));
            }

            return Result<VideoReportDetailsDto>.Success(details);
        }
    }
}
