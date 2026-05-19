using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed class GetChatsHandler
        : IRequestHandler<GetChatsQuery, Result<IReadOnlyList<ChatListItemDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IChatReadRepository _chatReadRepository;

        public GetChatsHandler(
            ICurrentUserLoader currentUserLoader,
            IChatReadRepository chatReadRepository)
        {
            _currentUserLoader = currentUserLoader;
            _chatReadRepository = chatReadRepository;
        }

        public async Task<Result<IReadOnlyList<ChatListItemDto>>> Handle(
            GetChatsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<ChatListItemDto>>.Failure(currentUserResult.Error);
            }

            var chats = currentUserResult.Value.Role switch
            {
                UserRole.Trainer => await _chatReadRepository.GetChatsForTrainerAsync(
                    currentUserResult.Value.Id,
                    cancellationToken),
                UserRole.Client => await _chatReadRepository.GetChatsForClientAsync(
                    currentUserResult.Value.Id,
                    cancellationToken),
                _ => null
            };

            if (chats is null)
            {
                return Result<IReadOnlyList<ChatListItemDto>>.Failure(
                    Error.Forbidden("chat.role_not_supported", "User cannot access chats"));
            }

            return Result<IReadOnlyList<ChatListItemDto>>.Success(chats);
        }
    }
}
