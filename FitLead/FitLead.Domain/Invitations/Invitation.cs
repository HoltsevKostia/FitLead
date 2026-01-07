using FitLead.Domain.Common;
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

        public static Invitation Create(
            Guid trainerId,
            Guid clientId,
            DateTime now)
        {
            if (trainerId == Guid.Empty)
                throw new ArgumentException("TrainerId is required");

            if (clientId == Guid.Empty)
                throw new ArgumentException("ClientId is required");

            return new Invitation(
                Guid.NewGuid(),
                trainerId,
                clientId,
                now,
                now.AddHours(48));
        }

        public void Accept(DateTime now)
        {
            EnsurePending();

            if (now > ExpiresAt)
                throw new InvalidOperationException("Invitation has expired");

            Status = InvitationStatus.Accepted;

            RaiseDomainEvent(new InvitationAcceptedDomainEvent(
            Id,
            TrainerId,
            ClientId));
        }

        public void Decline(DateTime now)
        {
            EnsurePending();

            if (now > ExpiresAt)
                throw new InvalidOperationException("Invitation has expired");

            Status = InvitationStatus.Declined;

            RaiseDomainEvent(new InvitationDeclinedDomainEvent(Id, TrainerId, ClientId));
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

        private void EnsurePending()
        {
            if (Status != InvitationStatus.Pending)
                throw new InvalidOperationException(
                    $"Invitation is already {Status}");
        }
    }
}
