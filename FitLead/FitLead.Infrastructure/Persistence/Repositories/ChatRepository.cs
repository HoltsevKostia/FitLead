using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Messenger.Chats;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ChatRepository : IChatRepository
    {
        private readonly FitLeadDbContext _context;

        public ChatRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<Chat?> GetByTrainerAndClientAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.Chats
                .FirstOrDefaultAsync(
                    chat => chat.TrainerId == trainerId &&
                            chat.ClientId == clientId,
                    cancellationToken);
        }

        public async Task<Chat?> GetByIdAsync(
            Guid chatId,
            CancellationToken cancellationToken)
        {
            return await _context.Chats
                .FirstOrDefaultAsync(chat => chat.Id == chatId, cancellationToken);
        }

        public async Task AddAsync(
            Chat chat,
            CancellationToken cancellationToken)
        {
            await _context.Chats.AddAsync(chat, cancellationToken);
        }
    }
}
