using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Messenger.Chats
{
    public sealed class Chat : AggregateRoot<Guid>
    {
        public Guid TrainerId { get; private set; }
        public Guid ClientId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? LastMessageAtUtc { get; private set; }

        private Chat() { }

        private Chat(
            Guid id,
            Guid trainerId,
            Guid clientId,
            DateTime createdAtUtc)
        {
            Id = id;
            TrainerId = trainerId;
            ClientId = clientId;
            CreatedAtUtc = createdAtUtc;
        }

        public static Result<Chat> Create(
            Guid trainerId,
            Guid clientId,
            DateTime createdAtUtc)
        {
            if (trainerId == Guid.Empty)
            {
                return Result<Chat>.Failure(
                    Error.Validation("chat.create.trainer_id_required", "TrainerId is required"));
            }

            if (clientId == Guid.Empty)
            {
                return Result<Chat>.Failure(
                    Error.Validation("chat.create.client_id_required", "ClientId is required"));
            }

            return Result<Chat>.Success(
                new Chat(
                    Guid.NewGuid(),
                    trainerId,
                    clientId,
                    createdAtUtc));
        }

        public bool HasParticipant(Guid userId)
        {
            return userId == TrainerId || userId == ClientId;
        }

        public void MarkMessageCreated(DateTime createdAtUtc)
        {
            if (!LastMessageAtUtc.HasValue || createdAtUtc > LastMessageAtUtc.Value)
            {
                LastMessageAtUtc = createdAtUtc;
            }
        }
    }
}
