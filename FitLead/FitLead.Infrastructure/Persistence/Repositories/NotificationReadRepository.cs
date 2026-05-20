using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Notifications.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class NotificationReadRepository : INotificationReadRepository
    {
        private readonly FitLeadDbContext _context;

        public NotificationReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetByRecipientAsync(
            Guid recipientUserId,
            int limit,
            CancellationToken cancellationToken)
        {
            if (limit <= 0)
            {
                return Array.Empty<NotificationDto>();
            }

            var projections = await _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.RecipientUserId == recipientUserId)
                .OrderByDescending(notification => notification.CreatedAtUtc)
                .ThenByDescending(notification => notification.Id)
                .Take(limit)
                .Select(notification => new NotificationProjection
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Title = notification.Title,
                    Body = notification.Body,
                    LinkUrl = notification.LinkUrl,
                    IsRead = notification.IsRead,
                    CreatedAtUtc = notification.CreatedAtUtc,
                    ReadAtUtc = notification.ReadAtUtc
                })
                .ToListAsync(cancellationToken);

            return projections
                .Select(ToDto)
                .ToList();
        }

        public async Task<int> GetUnreadCountAsync(
            Guid recipientUserId,
            CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .AsNoTracking()
                .CountAsync(
                    notification => notification.RecipientUserId == recipientUserId &&
                                    !notification.IsRead,
                    cancellationToken);
        }

        private static NotificationDto ToDto(NotificationProjection projection)
        {
            return new NotificationDto(
                projection.Id,
                projection.Type.ToString(),
                projection.Title,
                projection.Body,
                projection.LinkUrl,
                projection.IsRead,
                projection.CreatedAtUtc,
                projection.ReadAtUtc);
        }

        private sealed class NotificationProjection
        {
            public Guid Id { get; init; }
            public Domain.Notifications.NotificationType Type { get; init; }
            public required string Title { get; init; }
            public string? Body { get; init; }
            public required string LinkUrl { get; init; }
            public bool IsRead { get; init; }
            public DateTime CreatedAtUtc { get; init; }
            public DateTime? ReadAtUtc { get; init; }
        }
    }
}
