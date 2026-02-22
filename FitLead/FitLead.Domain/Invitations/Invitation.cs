using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Invitations.Events;

namespace FitLead.Domain.Invitations
{
    public sealed class Invitation : AggregateRoot<Guid>
    {
        public Guid TrainerId { get; private set; }
        public Guid ClientId { get; private set; }
        public InvitationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        private Invitation() { } // EF

        private Invitation(
            Guid id,
            Guid trainerId,
            Guid clientId,
            DateTime createdAt,
            DateTime expiresAt)
        {
            Id = id;
            TrainerId = trainerId;
            ClientId = clientId;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
            Status = InvitationStatus.Pending;
        }

        public static Result<Invitation> Create(
            Guid trainerId,
            Guid clientId,
            DateTime now)
        {
            if (trainerId == Guid.Empty)
                return Result<Invitation>.Failure(
                    Error.Validation("invitation.create.trainer_id_required", "TrainerId is required"));

            if (clientId == Guid.Empty)
                return Result<Invitation>.Failure(
                    Error.Validation("invitation.create.client_id_required", "ClientId is required"));

            return Result<Invitation>.Success(
                new Invitation(
                    Guid.NewGuid(),
                    trainerId,
                    clientId,
                    now,
                    now.AddHours(48)));
        }

        public Result Accept(DateTime now)
        {
            var pendingResult = EnsurePending();
            if (pendingResult.IsFailure)
                return pendingResult;

            if (now > ExpiresAt)
                return Result.Failure(
                    Error.Conflict("invitation.accept.expired", "Invitation has expired"));

            Status = InvitationStatus.Accepted;

            RaiseDomainEvent(new InvitationAcceptedDomainEvent(
            Id,
            TrainerId,
            ClientId));

            return Result.Success();
        }

        public Result Decline(DateTime now)
        {
            var pendingResult = EnsurePending();
            if (pendingResult.IsFailure)
                return pendingResult;

            if (now > ExpiresAt)
                return Result.Failure(
                    Error.Conflict("invitation.decline.expired", "Invitation has expired"));

            Status = InvitationStatus.Declined;

            RaiseDomainEvent(new InvitationDeclinedDomainEvent(Id, TrainerId, ClientId));

            return Result.Success();
        }

        public void Expire(DateTime now)
        {
            if (Status != InvitationStatus.Pending)
                return;

            if (now <= ExpiresAt)
                return;

            Status = InvitationStatus.Expired;

            RaiseDomainEvent(new InvitationExpiredDomainEvent(Id, TrainerId, ClientId));
        }

        private Result EnsurePending()
        {
            if (Status != InvitationStatus.Pending)
                return Result.Failure(
                    Error.Conflict("invitation.status.invalid_transition", $"Invitation is already {Status}"));

            return Result.Success();
        }
    }
}
