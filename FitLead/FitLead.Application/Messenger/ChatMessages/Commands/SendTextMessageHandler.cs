using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Application.Messenger.ChatMessages.Realtime;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.ChatMessages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitLead.Application.Messenger.ChatMessages.Commands
{
    public sealed class SendTextMessageHandler
        : IRequestHandler<SendTextMessageCommand, Result<ChatMessageDto>>
    {
        private readonly IChatLoader _chatLoader;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IChatRealtimeNotifier _chatRealtimeNotifier;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SendTextMessageHandler> _logger;

        public SendTextMessageHandler(
            IChatLoader chatLoader,
            IChatMessageRepository chatMessageRepository,
            ICurrentUserLoader currentUserLoader,
            IChatRealtimeNotifier chatRealtimeNotifier,
            IClock clock,
            IUnitOfWork unitOfWork,
            ILogger<SendTextMessageHandler> logger)
        {
            _chatLoader = chatLoader;
            _chatMessageRepository = chatMessageRepository;
            _currentUserLoader = currentUserLoader;
            _chatRealtimeNotifier = chatRealtimeNotifier;
            _clock = clock;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<ChatMessageDto>> Handle(
            SendTextMessageCommand request,
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

            var createdAtUtc = _clock.UtcNow;
            var messageResult = ChatMessage.CreateText(
                chatResult.Value,
                currentUserResult.Value.Id,
                request.Text,
                createdAtUtc);
            if (messageResult.IsFailure)
            {
                return Result<ChatMessageDto>.Failure(messageResult.Error);
            }

            chatResult.Value.MarkMessageCreated(createdAtUtc);
            await _chatMessageRepository.AddAsync(messageResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDto = ToDto(
                messageResult.Value,
                currentUserResult.Value.FullName);

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
                    "Failed to publish realtime message event for chat {ChatId} and message {MessageId}.",
                    messageDto.ChatId,
                    messageDto.Id);
            }

            return Result<ChatMessageDto>.Success(messageDto);
        }

        private static ChatMessageDto ToDto(
            ChatMessage message,
            string senderName)
        {
            return new ChatMessageDto(
                message.Id,
                message.ChatId,
                message.SenderId,
                senderName,
                message.Type.ToString(),
                message.Text,
                message.CreatedAtUtc);
        }
    }
}
