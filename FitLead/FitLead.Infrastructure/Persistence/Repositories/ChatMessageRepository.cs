using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Messenger.ChatMessages;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ChatMessageRepository : IChatMessageRepository
    {
        private readonly FitLeadDbContext _context;

        public ChatMessageRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            ChatMessage message,
            CancellationToken cancellationToken)
        {
            await _context.ChatMessages.AddAsync(message, cancellationToken);
        }
    }
}
