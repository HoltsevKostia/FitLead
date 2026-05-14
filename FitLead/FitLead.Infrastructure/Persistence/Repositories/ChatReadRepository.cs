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

        public async Task<ChatDetailsDto?> GetByIdAsync(
            Guid chatId,
            CancellationToken cancellationToken)
        {
            return await (
                from chat in _context.Chats.AsNoTracking()
                join trainer in _context.DomainUsers.AsNoTracking()
                    on chat.TrainerId equals trainer.Id
                join client in _context.DomainUsers.AsNoTracking()
                    on chat.ClientId equals client.Id
                where chat.Id == chatId
                select new ChatDetailsDto(
                    chat.Id,
                    chat.TrainerId,
                    trainer.FullName,
                    chat.ClientId,
                    client.FullName,
                    chat.CreatedAtUtc,
                    chat.LastMessageAtUtc))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ChatListItemDto>> GetChatsForTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await (
                from chat in _context.Chats.AsNoTracking()
                join trainerClient in _context.TrainerClients.AsNoTracking()
                    on new { chat.TrainerId, chat.ClientId }
                    equals new { trainerClient.TrainerId, trainerClient.ClientId }
                join trainer in _context.DomainUsers.AsNoTracking()
                    on chat.TrainerId equals trainer.Id
                join client in _context.DomainUsers.AsNoTracking()
                    on chat.ClientId equals client.Id
                where chat.TrainerId == trainerId
                orderby (chat.LastMessageAtUtc ?? chat.CreatedAtUtc) descending
                select new ChatListItemDto(
                    chat.Id,
                    chat.TrainerId,
                    trainer.FullName,
                    chat.ClientId,
                    client.FullName,
                    chat.LastMessageAtUtc))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ChatListItemDto>> GetChatsForClientAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await (
                from chat in _context.Chats.AsNoTracking()
                join trainerClient in _context.TrainerClients.AsNoTracking()
                    on new { chat.TrainerId, chat.ClientId }
                    equals new { trainerClient.TrainerId, trainerClient.ClientId }
                join trainer in _context.DomainUsers.AsNoTracking()
                    on chat.TrainerId equals trainer.Id
                join client in _context.DomainUsers.AsNoTracking()
                    on chat.ClientId equals client.Id
                where chat.ClientId == clientId
                orderby (chat.LastMessageAtUtc ?? chat.CreatedAtUtc) descending
                select new ChatListItemDto(
                    chat.Id,
                    chat.TrainerId,
                    trainer.FullName,
                    chat.ClientId,
                    client.FullName,
                    chat.LastMessageAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
