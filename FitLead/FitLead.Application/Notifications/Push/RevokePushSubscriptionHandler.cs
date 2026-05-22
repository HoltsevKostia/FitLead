using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Push
{
    public sealed class RevokePushSubscriptionHandler
        : IRequestHandler<RevokePushSubscriptionCommand, Result>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IPushSubscriptionRepository _subscriptionRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public RevokePushSubscriptionHandler(
            ICurrentUserLoader currentUserLoader,
            IPushSubscriptionRepository subscriptionRepository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _currentUserLoader = currentUserLoader;
            _subscriptionRepository = subscriptionRepository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RevokePushSubscriptionCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result.Failure(currentUserResult.Error);
            }

            var subscription = await _subscriptionRepository.GetByEndpointForUserAsync(
                request.Endpoint,
                currentUserResult.Value.Id,
                cancellationToken);

            if (subscription is null)
            {
                return Result.Success();
            }

            var revokeResult = subscription.Revoke(_clock.UtcNow);
            if (revokeResult.IsFailure)
            {
                return Result.Failure(revokeResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
