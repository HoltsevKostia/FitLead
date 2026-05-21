using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using FitLead.Domain.Notifications.PushSubscriptions;
using MediatR;

namespace FitLead.Application.Notifications.Push
{
    public sealed class RegisterPushSubscriptionHandler
        : IRequestHandler<RegisterPushSubscriptionCommand, Result<PushSubscriptionDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IPushSubscriptionRepository _subscriptionRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterPushSubscriptionHandler(
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

        public async Task<Result<PushSubscriptionDto>> Handle(
            RegisterPushSubscriptionCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<PushSubscriptionDto>.Failure(currentUserResult.Error);
            }

            var currentUser = currentUserResult.Value;
            var existingSubscription = await _subscriptionRepository.GetByEndpointAsync(
                request.Endpoint,
                cancellationToken);

            if (existingSubscription is null)
            {
                var createResult = PushSubscription.Create(
                    currentUser.Id,
                    request.Endpoint,
                    request.P256dh,
                    request.Auth,
                    request.UserAgent,
                    _clock.UtcNow);
                if (createResult.IsFailure)
                {
                    return Result<PushSubscriptionDto>.Failure(createResult.Error);
                }

                await _subscriptionRepository.AddAsync(createResult.Value, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<PushSubscriptionDto>.Success(ToDto(createResult.Value));
            }

            var refreshResult = existingSubscription.Refresh(
                currentUser.Id,
                request.P256dh,
                request.Auth,
                request.UserAgent);
            if (refreshResult.IsFailure)
            {
                return Result<PushSubscriptionDto>.Failure(refreshResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<PushSubscriptionDto>.Success(ToDto(existingSubscription));
        }

        private static PushSubscriptionDto ToDto(PushSubscription subscription)
        {
            return new PushSubscriptionDto(
                subscription.Id,
                subscription.Endpoint,
                subscription.CreatedAtUtc);
        }
    }
}
