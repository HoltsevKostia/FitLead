using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Application.Messenger.Chats.Queries;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.Chats;
using MediatR;

namespace FitLead.Application.Messenger.Chats.Commands
{
    public sealed class GetOrCreateChatWithClientHandler
        : IRequestHandler<GetOrCreateChatWithClientCommand, Result<ChatDto>>
    {
        private readonly IChatLoader _chatLoader;
        private readonly IChatRepository _chatRepository;
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public GetOrCreateChatWithClientHandler(
            IChatLoader chatLoader,
            IChatRepository chatRepository,
            ICurrentUserLoader currentUserLoader,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _chatLoader = chatLoader;
            _chatRepository = chatRepository;
            _currentUserLoader = currentUserLoader;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ChatDto>> Handle(
            GetOrCreateChatWithClientCommand request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _chatLoader.EnsureCurrentTrainerHasClientAsync(
                request.ClientId,
                cancellationToken);
            if (accessResult.IsFailure)
            {
                return Result<ChatDto>.Failure(accessResult.Error);
            }

            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ChatDto>.Failure(currentUserResult.Error);
            }

            var trainerId = currentUserResult.Value.Id;
            var existingChat = await _chatRepository.GetByTrainerAndClientAsync(
                trainerId,
                request.ClientId,
                cancellationToken);
            if (existingChat is not null)
            {
                return Result<ChatDto>.Success(ToDto(existingChat));
            }

            var chatResult = Chat.Create(
                trainerId,
                request.ClientId,
                _clock.UtcNow);
            if (chatResult.IsFailure)
            {
                return Result<ChatDto>.Failure(chatResult.Error);
            }

            await _chatRepository.AddAsync(chatResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ChatDto>.Success(ToDto(chatResult.Value));
        }

        private static ChatDto ToDto(Chat chat)
        {
            return new ChatDto(
                chat.Id,
                chat.TrainerId,
                chat.ClientId,
                chat.CreatedAtUtc,
                chat.LastMessageAtUtc);
        }
    }
}
