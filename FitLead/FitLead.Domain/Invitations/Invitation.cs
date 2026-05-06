using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Invitations
{
    public sealed class Invitation : AggregateRoot<Guid>
    {
        public Guid TrainerId { get; private set; }
        public string TokenHash { get; private set; } = null!;
        public InvitationStatus Status { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }
        public Guid? AcceptedByClientId { get; private set; }
        public DateTime? AcceptedAtUtc { get; private set; }

        private Invitation()
        {
        }

        private Invitation(
            Guid id,
            Guid trainerId,
            string tokenHash,
            DateTime createdAtUtc,
            DateTime expiresAtUtc)
        {
            Id = id;
            TrainerId = trainerId;
            TokenHash = tokenHash;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
            Status = InvitationStatus.Pending;
        }

        public static Result<Invitation> Create(
            Guid trainerId,
            string tokenHash,
            DateTime createdAtUtc,
            DateTime expiresAtUtc)
        {
            if (trainerId == Guid.Empty)
            {
                return Result<Invitation>.Failure(
                    Error.Validation("invitation.create.trainer_id_required", "TrainerId is required"));
            }

            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                return Result<Invitation>.Failure(
                    Error.Validation("invitation.create.token_hash_required", "TokenHash is required"));
            }

            if (expiresAtUtc <= createdAtUtc)
            {
                return Result<Invitation>.Failure(
                    Error.Validation("invitation.create.expires_at_invalid", "ExpiresAtUtc must be after CreatedAtUtc"));
            }

            return Result<Invitation>.Success(
                new Invitation(
                    Guid.NewGuid(),
                    trainerId,
                    tokenHash.Trim(),
                    createdAtUtc,
                    expiresAtUtc));
        }

        public Result Accept(Guid clientId, DateTime acceptedAtUtc)
        {
            if (clientId == Guid.Empty)
            {
                return Result.Failure(
                    Error.Validation("invitation.accept.client_id_required", "ClientId is required"));
            }

            if (Status == InvitationStatus.Accepted)
            {
                if (AcceptedByClientId == clientId)
                {
                    return Result.Success();
                }

                return Result.Failure(
                    Error.Conflict("invitation.accept.already_accepted", "Invitation has already been accepted"));
            }

            if (Status == InvitationStatus.Revoked)
            {
                return Result.Failure(
                    Error.Conflict("invitation.accept.revoked", "Invitation has been revoked"));
            }

            var pendingResult = EnsurePending();
            if (pendingResult.IsFailure)
            {
                return pendingResult;
            }

            if (IsExpired(acceptedAtUtc))
            {
                return Result.Failure(
                    Error.Conflict("invitation.accept.expired", "Invitation has expired"));
            }

            Status = InvitationStatus.Accepted;
            AcceptedByClientId = clientId;
            AcceptedAtUtc = acceptedAtUtc;

            return Result.Success();
        }

        public Result Revoke(DateTime revokedAtUtc)
        {
            var pendingResult = EnsurePending();
            if (pendingResult.IsFailure)
            {
                return pendingResult;
            }

            if (IsExpired(revokedAtUtc))
            {
                return Result.Failure(
                    Error.Conflict("invitation.revoke.expired", "Invitation has expired"));
            }

            Status = InvitationStatus.Revoked;

            return Result.Success();
        }

        public bool IsExpired(DateTime utcNow)
            => Status == InvitationStatus.Pending && ExpiresAtUtc <= utcNow;

        private Result EnsurePending()
        {
            if (Status != InvitationStatus.Pending)
            {
                return Result.Failure(
                    Error.Conflict("invitation.status.invalid_transition", $"Invitation is already {Status}"));
            }

            return Result.Success();
        }
    }
}
