using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Common.Time;
using FitLead.Application.Messenger.ChatMessages.Outbox;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.ChatMessages;
using MediatR;

namespace FitLead.Application.Messenger.ChatMessages.Commands
{
    public sealed class SendTextMessageHandler
        : IRequestHandler<SendTextMessageCommand, Result<ChatMessageDto>>
    {
        private readonly IChatLoader _chatLoader;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IOutbox _outbox;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public SendTextMessageHandler(
            IChatLoader chatLoader,
            IChatMessageRepository chatMessageRepository,
            ICurrentUserLoader currentUserLoader,
            IOutbox outbox,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _chatLoader = chatLoader;
            _chatMessageRepository = chatMessageRepository;
            _currentUserLoader = currentUserLoader;
            _outbox = outbox;
            _clock = clock;
            _unitOfWork = unitOfWork;
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
            await _outbox.EnqueueAsync(
                OutboxEventTypes.Messenger.ChatMessageCreated,
                new ChatMessageCreatedOutboxPayload(
                    messageResult.Value.ChatId,
                    messageResult.Value.Id),
                createdAtUtc,
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDto = ToDto(
                messageResult.Value,
                currentUserResult.Value.FullName);

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
                null,
                message.CreatedAtUtc);
        }
    }
}
