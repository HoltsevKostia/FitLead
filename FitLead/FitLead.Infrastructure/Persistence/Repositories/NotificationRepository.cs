using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class NotificationRepository : INotificationRepository
    {
        private readonly FitLeadDbContext _context;

        public NotificationRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Notification notification,
            CancellationToken cancellationToken)
        {
            await _context.Notifications.AddAsync(notification, cancellationToken);
        }

        public async Task<Notification?> GetByIdForRecipientAsync(
            Guid notificationId,
            Guid recipientUserId,
            CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(
                    notification => notification.Id == notificationId &&
                                    notification.RecipientUserId == recipientUserId,
                    cancellationToken);
        }

        public async Task<Notification?> GetBySourceEventAsync(
            Guid sourceEventId,
            Guid recipientUserId,
            NotificationType type,
            CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(
                    notification => notification.SourceEventId == sourceEventId &&
                                    notification.RecipientUserId == recipientUserId &&
                                    notification.Type == type,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Notification>> GetUnreadByRecipientAsync(
            Guid recipientUserId,
            CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .Where(notification => notification.RecipientUserId == recipientUserId &&
                                       !notification.IsRead)
                .ToListAsync(cancellationToken);
        }
    }
}
