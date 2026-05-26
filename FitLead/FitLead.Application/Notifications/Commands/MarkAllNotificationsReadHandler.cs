using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Commands
{
    public sealed class MarkAllNotificationsReadHandler
        : IRequestHandler<MarkAllNotificationsReadCommand, Result>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly INotificationRepository _notificationRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public MarkAllNotificationsReadHandler(
            ICurrentUserLoader currentUserLoader,
            INotificationRepository notificationRepository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _currentUserLoader = currentUserLoader;
            _notificationRepository = notificationRepository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            MarkAllNotificationsReadCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result.Failure(currentUserResult.Error);
            }

            var unreadNotifications = await _notificationRepository.GetUnreadByRecipientAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            var utcNow = _clock.UtcNow;
            foreach (var notification in unreadNotifications)
            {
                var markReadResult = notification.MarkRead(utcNow);
                if (markReadResult.IsFailure)
                {
                    return markReadResult;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
