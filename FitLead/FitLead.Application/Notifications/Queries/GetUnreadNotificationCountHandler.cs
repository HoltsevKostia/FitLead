using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Queries
{
    public sealed class GetUnreadNotificationCountHandler
        : IRequestHandler<GetUnreadNotificationCountQuery, Result<UnreadNotificationCountDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly INotificationReadRepository _notificationReadRepository;

        public GetUnreadNotificationCountHandler(
            ICurrentUserLoader currentUserLoader,
            INotificationReadRepository notificationReadRepository)
        {
            _currentUserLoader = currentUserLoader;
            _notificationReadRepository = notificationReadRepository;
        }

        public async Task<Result<UnreadNotificationCountDto>> Handle(
            GetUnreadNotificationCountQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<UnreadNotificationCountDto>.Failure(currentUserResult.Error);
            }

            var count = await _notificationReadRepository.GetUnreadCountAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            return Result<UnreadNotificationCountDto>.Success(
                new UnreadNotificationCountDto(count));
        }
    }
}
