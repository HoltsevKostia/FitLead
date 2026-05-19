using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Media.MediaAssets.Access;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Application.Messenger.ChatMessages.Realtime;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

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
        private readonly IChatRealtimeNotifier _chatRealtimeNotifier;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateVideoReportHandler> _logger;

        public CreateVideoReportHandler(
            IChatLoader chatLoader,
            ICurrentUserLoader currentUserLoader,
            IMediaAssetLoader mediaAssetLoader,
            IVideoReportRepository videoReportRepository,
            IChatMessageRepository chatMessageRepository,
            IChatRealtimeNotifier chatRealtimeNotifier,
            IClock clock,
            IUnitOfWork unitOfWork,
            ILogger<CreateVideoReportHandler> logger)
        {
            _chatLoader = chatLoader;
            _currentUserLoader = currentUserLoader;
            _mediaAssetLoader = mediaAssetLoader;
            _videoReportRepository = videoReportRepository;
            _chatMessageRepository = chatMessageRepository;
            _chatRealtimeNotifier = chatRealtimeNotifier;
            _clock = clock;
            _unitOfWork = unitOfWork;
            _logger = logger;
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDto = ToDto(
                messageResult.Value,
                currentUser.FullName,
                videoReportResult.Value);

            try
            {
                await _chatRealtimeNotifier.MessageCreatedAsync(
                    messageDto, 
                    CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to publish realtime video report message event for chat {ChatId} and message {MessageId}.",
                    messageDto.ChatId,
                    messageDto.Id);
            }

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
