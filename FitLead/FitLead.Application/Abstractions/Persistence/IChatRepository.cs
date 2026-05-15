using FitLead.Domain.Messenger.Chats;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IChatRepository
    {
        Task<Chat?> GetByTrainerAndClientAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);

        Task<Chat?> GetByIdAsync(
            Guid chatId,
            CancellationToken cancellationToken);

        Task AddAsync(
            Chat chat,
            CancellationToken cancellationToken);
    }
}
