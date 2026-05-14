using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Messenger.ChatMessages.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ChatMessageReadRepository : IChatMessageReadRepository
    {
        private readonly FitLeadDbContext _context;

        public ChatMessageReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ChatMessageHistoryItemDto>> GetMessagesAsync(
            Guid chatId,
            int limit,
            DateTime? beforeCreatedAtUtc,
            CancellationToken cancellationToken)
        {
            var query =
                from message in _context.ChatMessages.AsNoTracking()
                join sender in _context.DomainUsers.AsNoTracking()
                    on message.SenderId equals sender.Id
                where message.ChatId == chatId
                select new
                {
                    Message = message,
                    SenderName = sender.FullName
                };

            if (beforeCreatedAtUtc.HasValue)
            {
                query = query.Where(x => x.Message.CreatedAtUtc < beforeCreatedAtUtc.Value);
            }

            return await query
                .OrderByDescending(x => x.Message.CreatedAtUtc)
                .ThenByDescending(x => x.Message.Id)
                .Take(limit)
                .Select(x => new ChatMessageHistoryItemDto(
                    x.Message.Id,
                    x.Message.ChatId,
                    x.Message.SenderId,
                    x.SenderName,
                    x.Message.Type.ToString(),
                    x.Message.Text,
                    x.Message.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
