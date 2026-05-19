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

        public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(
            Guid chatId,
            int limit,
            DateTime? beforeCreatedAtUtc,
            CancellationToken cancellationToken)
        {
            var mediaCounts =
                from media in _context.VideoReportMedia.AsNoTracking()
                group media by media.VideoReportId into mediaGroup
                select new
                {
                    VideoReportId = mediaGroup.Key,
                    Count = mediaGroup.Count()
                };

            var query =
                from message in _context.ChatMessages.AsNoTracking()
                join sender in _context.DomainUsers.AsNoTracking()
                    on message.SenderId equals sender.Id
                join videoReport in _context.VideoReports.AsNoTracking()
                    on message.VideoReportId equals videoReport.Id into videoReports
                from videoReport in videoReports.DefaultIfEmpty()
                join mediaCount in mediaCounts
                    on message.VideoReportId equals mediaCount.VideoReportId into reportMediaCounts
                from mediaCount in reportMediaCounts.DefaultIfEmpty()
                where message.ChatId == chatId
                select new
                {
                    Message = message,
                    SenderName = sender.FullName,
                    VideoReport = videoReport,
                    MediaCount = mediaCount
                };

            if (beforeCreatedAtUtc.HasValue)
            {
                query = query.Where(x => x.Message.CreatedAtUtc < beforeCreatedAtUtc.Value);
            }

            return await query
                .OrderByDescending(x => x.Message.CreatedAtUtc)
                .ThenByDescending(x => x.Message.Id)
                .Take(limit)
                .Select(x => new ChatMessageDto(
                    x.Message.Id,
                    x.Message.ChatId,
                    x.Message.SenderId,
                    x.SenderName,
                    x.Message.Type.ToString(),
                    x.Message.Text,
                    x.VideoReport == null
                        ? null
                        : new VideoReportPreviewDto(
                            x.VideoReport.Id,
                            x.VideoReport.Title,
                            x.VideoReport.Description,
                            x.VideoReport.Status.ToString(),
                            x.MediaCount == null ? 0 : x.MediaCount.Count),
                    x.Message.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
