using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Application.Messenger.VideoReports.Queries;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Messenger.VideoReports.Commands
{
    public sealed class SubmitVideoReportFeedbackHandler
        : IRequestHandler<SubmitVideoReportFeedbackCommand, Result<VideoReportDetailsDto>>
    {
        private readonly IChatLoader _chatLoader;
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IVideoReportRepository _videoReportRepository;
        private readonly IVideoReportReadRepository _videoReportReadRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitVideoReportFeedbackHandler(
            IChatLoader chatLoader,
            ICurrentUserLoader currentUserLoader,
            IVideoReportRepository videoReportRepository,
            IVideoReportReadRepository videoReportReadRepository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _chatLoader = chatLoader;
            _currentUserLoader = currentUserLoader;
            _videoReportRepository = videoReportRepository;
            _videoReportReadRepository = videoReportReadRepository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<VideoReportDetailsDto>> Handle(
            SubmitVideoReportFeedbackCommand request,
            CancellationToken cancellationToken)
        {
            var chatResult = await _chatLoader.GetAccessibleOrNotFoundAsync(
                request.ChatId,
                cancellationToken);
            if (chatResult.IsFailure)
            {
                return Result<VideoReportDetailsDto>.Failure(chatResult.Error);
            }

            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<VideoReportDetailsDto>.Failure(currentUserResult.Error);
            }

            var currentUser = currentUserResult.Value;
            if (currentUser.Role != UserRole.Trainer ||
                chatResult.Value.TrainerId != currentUser.Id)
            {
                return Result<VideoReportDetailsDto>.Failure(
                    Error.NotFound("chat.not_found", "Chat not found"));
            }

            var report = await _videoReportRepository.GetByIdAndChatIdAsync(
                request.ReportId,
                request.ChatId,
                cancellationToken);
            if (report is null)
            {
                return Result<VideoReportDetailsDto>.Failure(
                    Error.NotFound("video_report.not_found", "Video report not found"));
            }

            var reviewResult = report.Review(request.Text, _clock.UtcNow);
            if (reviewResult.IsFailure)
            {
                return Result<VideoReportDetailsDto>.Failure(reviewResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
