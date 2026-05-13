using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Messenger.Chats.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ChatReadRepository : IChatReadRepository
    {
        private readonly FitLeadDbContext _context;

        public ChatReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ChatDto>> GetChatsForTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await GetChatsQuery()
                .Where(chat => chat.TrainerId == trainerId)
                .OrderByDescending(chat => chat.LastMessageAtUtc ?? chat.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ChatDto>> GetChatsForClientAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await GetChatsQuery()
                .Where(chat => chat.ClientId == clientId)
                .OrderByDescending(chat => chat.LastMessageAtUtc ?? chat.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        private IQueryable<ChatDto> GetChatsQuery()
        {
            return
                from chat in _context.Chats.AsNoTracking()
                select new ChatDto(
                    chat.Id,
                    chat.TrainerId,
                    chat.ClientId,
                    chat.CreatedAtUtc,
                    chat.LastMessageAtUtc);
        }
    }
}
