using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Queries
{
    public sealed class GetNotificationsHandler
        : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
    {
        private const int DefaultLimit = 50;
        private const int MaxLimit = 100;

        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly INotificationReadRepository _notificationReadRepository;

        public GetNotificationsHandler(
            ICurrentUserLoader currentUserLoader,
            INotificationReadRepository notificationReadRepository)
        {
            _currentUserLoader = currentUserLoader;
            _notificationReadRepository = notificationReadRepository;
        }

        public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(
            GetNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<NotificationDto>>.Failure(currentUserResult.Error);
            }

            var limit = Math.Clamp(
                request.Limit ?? DefaultLimit,
                1,
                MaxLimit);

            var notifications = await _notificationReadRepository.GetByRecipientAsync(
                currentUserResult.Value.Id,
                limit,
                cancellationToken);

            return Result<IReadOnlyList<NotificationDto>>.Success(notifications);
        }
    }
}
