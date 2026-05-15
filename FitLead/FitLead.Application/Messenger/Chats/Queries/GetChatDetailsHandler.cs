using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed class GetChatDetailsHandler
        : IRequestHandler<GetChatDetailsQuery, Result<ChatDetailsDto>>
    {
        private readonly IChatLoader _chatLoader;
        private readonly IChatReadRepository _chatReadRepository;

        public GetChatDetailsHandler(
            IChatLoader chatLoader,
            IChatReadRepository chatReadRepository)
        {
            _chatLoader = chatLoader;
            _chatReadRepository = chatReadRepository;
        }

        public async Task<Result<ChatDetailsDto>> Handle(
            GetChatDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var chatResult = await _chatLoader.GetAccessibleOrNotFoundAsync(
                request.ChatId,
                cancellationToken);
            if (chatResult.IsFailure)
            {
                return Result<ChatDetailsDto>.Failure(chatResult.Error);
            }

            var chat = await _chatReadRepository.GetByIdAsync(
                request.ChatId,
                cancellationToken);
            if (chat is null)
            {
                return Result<ChatDetailsDto>.Failure(
                    Error.NotFound("chat.not_found", "Chat not found"));
            }

            return Result<ChatDetailsDto>.Success(chat);
        }
    }
}
