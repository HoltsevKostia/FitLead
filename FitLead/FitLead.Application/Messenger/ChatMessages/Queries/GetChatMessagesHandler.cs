using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed class GetChatMessagesHandler
        : IRequestHandler<GetChatMessagesQuery, Result<ChatMessageHistoryDto>>
    {
        private const int DefaultLimit = 50;
        private const int MaxLimit = 100;

        private readonly IChatLoader _chatLoader;
        private readonly IChatMessageReadRepository _chatMessageReadRepository;

        public GetChatMessagesHandler(
            IChatLoader chatLoader,
            IChatMessageReadRepository chatMessageReadRepository)
        {
            _chatLoader = chatLoader;
            _chatMessageReadRepository = chatMessageReadRepository;
        }

        public async Task<Result<ChatMessageHistoryDto>> Handle(
            GetChatMessagesQuery request,
            CancellationToken cancellationToken)
        {
            var chatResult = await _chatLoader.GetAccessibleOrNotFoundAsync(
                request.ChatId,
                cancellationToken);
            if (chatResult.IsFailure)
            {
                return Result<ChatMessageHistoryDto>.Failure(chatResult.Error);
            }

            var limit = ClampLimit(request.Limit);
            var messages = await _chatMessageReadRepository.GetMessagesAsync(
                request.ChatId,
                limit + 1,
                request.BeforeCreatedAtUtc,
                cancellationToken);

            var hasMore = messages.Count > limit;
            var items = messages
                .Take(limit)
                .Reverse()
                .ToArray();

            return Result<ChatMessageHistoryDto>.Success(
                new ChatMessageHistoryDto(items, hasMore));
        }

        private static int ClampLimit(int? limit)
        {
            if (!limit.HasValue)
            {
                return DefaultLimit;
            }

            return Math.Clamp(limit.Value, 1, MaxLimit);
        }
    }
}
