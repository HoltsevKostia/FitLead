using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Modules.Users;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Users;

namespace FitLead.Application.Messenger.Chats.Access
{
    public sealed class ChatLoader : IChatLoader
    {
        private static readonly Error ChatNotFound =
            Error.NotFound("chat.not_found", "Chat not found");

        private readonly IChatRepository _chatRepository;
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IUsersModule _usersModule;

        public ChatLoader(
            IChatRepository chatRepository,
            ICurrentUserLoader currentUserLoader,
            IUsersModule usersModule)
        {
            _chatRepository = chatRepository;
            _currentUserLoader = currentUserLoader;
            _usersModule = usersModule;
        }

        public async Task<Result<Chat>> GetAccessibleOrNotFoundAsync(
            Guid chatId,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<Chat>.Failure(currentUserResult.Error);
            }

            return await GetAccessibleForUserOrNotFoundAsync(
                currentUserResult.Value.Id,
                chatId,
                cancellationToken);
        }

        public async Task<Result<Chat>> GetAccessibleForUserOrNotFoundAsync(
            Guid userId,
            Guid chatId,
            CancellationToken cancellationToken)
        {
            var user = await _usersModule.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return Result<Chat>.Failure(ChatNotFound);
            }

            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
            if (chat is null)
            {
                return Result<Chat>.Failure(ChatNotFound);
            }

            var hasAccess = user.Role switch
            {
                UserRole.Trainer => await HasTrainerAccessAsync(
                    user.Id,
                    chat,
                    cancellationToken),
                UserRole.Client => await HasClientAccessAsync(
                    user.Id,
                    chat,
                    cancellationToken),
                _ => false
            };

            if (!hasAccess)
            {
                return Result<Chat>.Failure(ChatNotFound);
            }

            return Result<Chat>.Success(chat);
        }

        public async Task<Result> EnsureCurrentTrainerHasClientAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Trainer)
            {
                return Result.Failure(ChatNotFound);
            }

            var hasRelationship = await _usersModule.HasTrainerClientRelationshipAsync(
                currentUserResult.Value.Id,
                clientId,
                cancellationToken);

            return hasRelationship
                ? Result.Success()
                : Result.Failure(ChatNotFound);
        }

        public async Task<Result> EnsureCurrentClientHasTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result.Failure(ChatNotFound);
            }

            var hasRelationship = await _usersModule.HasTrainerClientRelationshipAsync(
                trainerId,
                currentUserResult.Value.Id,
                cancellationToken);

            return hasRelationship
                ? Result.Success()
                : Result.Failure(ChatNotFound);
        }

        private Task<bool> HasTrainerAccessAsync(
            Guid trainerId,
            Chat chat,
            CancellationToken cancellationToken)
        {
            if (chat.TrainerId != trainerId)
            {
                return Task.FromResult(false);
            }

            return _usersModule.HasTrainerClientRelationshipAsync(
                chat.TrainerId,
                chat.ClientId,
                cancellationToken);
        }

        private Task<bool> HasClientAccessAsync(
            Guid clientId,
            Chat chat,
            CancellationToken cancellationToken)
        {
            if (chat.ClientId != clientId)
            {
                return Task.FromResult(false);
            }

            return _usersModule.HasTrainerClientRelationshipAsync(
                chat.TrainerId,
                chat.ClientId,
                cancellationToken);
        }
    }
}
