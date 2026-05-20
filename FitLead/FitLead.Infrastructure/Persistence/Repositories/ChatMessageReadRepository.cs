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

        public async Task<ChatMessageDto?> GetMessageAsync(
            Guid messageId,
            CancellationToken cancellationToken)
        {
            var projection = await BuildMessageDtoQuery()
                .Where(x => x.Message.Id == messageId)
                .FirstOrDefaultAsync(cancellationToken);

            return projection is null
                ? null
                : ToDto(projection);
        }

        public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(
            Guid chatId,
            int limit,
            DateTime? beforeCreatedAtUtc,
            CancellationToken cancellationToken)
        {
            var query = BuildMessageDtoQuery()
                .Where(x => x.Message.ChatId == chatId);

            if (beforeCreatedAtUtc.HasValue)
            {
                query = query.Where(x => x.Message.CreatedAtUtc < beforeCreatedAtUtc.Value);
            }

            var projections = await query
                .OrderByDescending(x => x.Message.CreatedAtUtc)
                .ThenByDescending(x => x.Message.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return projections
                .Select(ToDto)
                .ToList();
        }

        private static ChatMessageDto ToDto(MessageDtoProjection projection)
        {
            return new ChatMessageDto(
                projection.Message.Id,
                projection.Message.ChatId,
                projection.Message.SenderId,
                projection.SenderName,
                projection.Message.Type.ToString(),
                projection.Message.Text,
                projection.VideoReport == null
                    ? null
                    : new VideoReportPreviewDto(
                        projection.VideoReport.Id,
                        projection.VideoReport.Title,
                        projection.VideoReport.Description,
                        projection.VideoReport.Status.ToString(),
                        projection.MediaCount),
                projection.Message.CreatedAtUtc);
        }

        private IQueryable<MessageDtoProjection> BuildMessageDtoQuery()
        {
            return
                from message in _context.ChatMessages.AsNoTracking()
                join sender in _context.DomainUsers.AsNoTracking()
                    on message.SenderId equals sender.Id
                join videoReport in _context.VideoReports.AsNoTracking()
                    on message.VideoReportId equals videoReport.Id into videoReports
                from videoReport in videoReports.DefaultIfEmpty()
                select new MessageDtoProjection
                {
                    Message = message,
                    SenderName = sender.FullName,
                    VideoReport = videoReport,
                    MediaCount = message.VideoReportId == null
                        ? 0
                        : _context.VideoReportMedia.AsNoTracking()
                            .Count(media => media.VideoReportId == message.VideoReportId)
                };
        }

        private sealed class MessageDtoProjection
        {
            public required Domain.Messenger.ChatMessages.ChatMessage Message { get; init; }
            public required string SenderName { get; init; }
            public Domain.Messenger.VideoReports.VideoReport? VideoReport { get; init; }
            public int MediaCount { get; init; }
        }
    }
}
