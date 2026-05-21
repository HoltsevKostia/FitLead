using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Commands
{
    public sealed class MarkNotificationReadHandler
        : IRequestHandler<MarkNotificationReadCommand, Result>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly INotificationRepository _notificationRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public MarkNotificationReadHandler(
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
            MarkNotificationReadCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result.Failure(currentUserResult.Error);
            }

            var notification = await _notificationRepository.GetByIdForRecipientAsync(
                request.NotificationId,
                currentUserResult.Value.Id,
                cancellationToken);
            if (notification is null)
            {
                return Result.Failure(
                    Error.NotFound("notification.not_found", "Notification not found"));
            }

            var markReadResult = notification.MarkRead(_clock.UtcNow);
            if (markReadResult.IsFailure)
            {
                return markReadResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
