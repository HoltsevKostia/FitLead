using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Common.Time;
using FitLead.Application.Media.MediaAssets.Access;
using FitLead.Application.Messenger.ChatMessages.Outbox;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Application.Messenger.VideoReports.Outbox;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Messenger.VideoReports.Commands
{
    public sealed class CreateVideoReportHandler
        : IRequestHandler<CreateVideoReportCommand, Result<ChatMessageDto>>
    {
        private readonly IChatLoader _chatLoader;
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IMediaAssetLoader _mediaAssetLoader;
        private readonly IVideoReportRepository _videoReportRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IOutbox _outbox;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public CreateVideoReportHandler(
            IChatLoader chatLoader,
            ICurrentUserLoader currentUserLoader,
            IMediaAssetLoader mediaAssetLoader,
            IVideoReportRepository videoReportRepository,
            IChatMessageRepository chatMessageRepository,
            IOutbox outbox,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _chatLoader = chatLoader;
            _currentUserLoader = currentUserLoader;
            _mediaAssetLoader = mediaAssetLoader;
            _videoReportRepository = videoReportRepository;
            _chatMessageRepository = chatMessageRepository;
            _outbox = outbox;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ChatMessageDto>> Handle(
            CreateVideoReportCommand request,
            CancellationToken cancellationToken)
        {
            var chatResult = await _chatLoader.GetAccessibleOrNotFoundAsync(
                request.ChatId,
                cancellationToken);
            if (chatResult.IsFailure)
            {
                return Result<ChatMessageDto>.Failure(chatResult.Error);
            }

            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ChatMessageDto>.Failure(currentUserResult.Error);
            }

            var currentUser = currentUserResult.Value;
            var chat = chatResult.Value;
            if (currentUser.Role != UserRole.Client || chat.ClientId != currentUser.Id)
            {
                return Result<ChatMessageDto>.Failure(
                    Error.NotFound("chat.not_found", "Chat not found"));
            }

            if (request.MediaAssetIds is null)
            {
                return Result<ChatMessageDto>.Failure(
                    Error.Validation(
                        "video_report.create.media_required",
                        "At least one media asset is required"));
            }

            if (request.MediaAssetIds.Count != request.MediaAssetIds.Distinct().Count())
            {
                return Result<ChatMessageDto>.Failure(
                    Error.Validation(
                        "video_report.create.duplicate_media_assets",
                        "Video report cannot contain duplicate media assets"));
            }

            var mediaAssetsResult = await _mediaAssetLoader.GetOwnedAllowedForVideoReportOrNotFoundAsync(
                currentUser.Id,
                request.MediaAssetIds,
                cancellationToken);
            if (mediaAssetsResult.IsFailure)
            {
                return Result<ChatMessageDto>.Failure(mediaAssetsResult.Error);
            }

            var createdAtUtc = _clock.UtcNow;
            var videoReportResult = VideoReport.Create(
                chat.Id,
                currentUser.Id,
                chat.TrainerId,
                request.Title,
                request.Description,
                request.MediaAssetIds,
                createdAtUtc);
            if (videoReportResult.IsFailure)
            {
                return Result<ChatMessageDto>.Failure(videoReportResult.Error);
            }

            var messageResult = ChatMessage.CreateVideoReport(
                chat,
                videoReportResult.Value,
                currentUser.Id,
                createdAtUtc);
            if (messageResult.IsFailure)
            {
                return Result<ChatMessageDto>.Failure(messageResult.Error);
            }

            chat.MarkMessageCreated(createdAtUtc);
            await _videoReportRepository.AddAsync(videoReportResult.Value, cancellationToken);
            await _chatMessageRepository.AddAsync(messageResult.Value, cancellationToken);
            await _outbox.EnqueueAsync(
                OutboxEventTypes.Messenger.ChatMessageCreated,
                new ChatMessageCreatedOutboxPayload(
                    messageResult.Value.ChatId,
                    messageResult.Value.Id),
                createdAtUtc,
                cancellationToken);
            await _outbox.EnqueueAsync(
                OutboxEventTypes.Messenger.VideoReportSubmitted,
                new VideoReportSubmittedOutboxPayload(
                    chat.Id,
                    videoReportResult.Value.Id,
                    currentUser.Id,
                    chat.TrainerId,
                    videoReportResult.Value.Title,
                    createdAtUtc),
                createdAtUtc,
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDto = ToDto(
                messageResult.Value,
                currentUser.FullName,
                videoReportResult.Value);

            return Result<ChatMessageDto>.Success(messageDto);
        }

        private static ChatMessageDto ToDto(
            ChatMessage message,
            string senderName,
            VideoReport videoReport)
        {
            return new ChatMessageDto(
                message.Id,
                message.ChatId,
                message.SenderId,
                senderName,
                message.Type.ToString(),
                message.Text,
                new VideoReportPreviewDto(
                    videoReport.Id,
                    videoReport.Title,
                    videoReport.Description,
                    videoReport.Status.ToString(),
                    videoReport.Media.Count),
                message.CreatedAtUtc);
        }
    }
}
