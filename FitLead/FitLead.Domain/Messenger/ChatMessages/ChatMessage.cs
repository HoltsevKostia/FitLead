using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.Chats;

namespace FitLead.Domain.Messenger.ChatMessages
{
    public sealed class ChatMessage : Entity<Guid>
    {
        public const int MaxTextLength = 4000;

        public Guid ChatId { get; private set; }
        public Guid SenderId { get; private set; }
        public ChatMessageType Type { get; private set; }
        public string? Text { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        private ChatMessage() { }

        private ChatMessage(
            Guid id,
            Guid chatId,
            Guid senderId,
            ChatMessageType type,
            string? text,
            DateTime createdAtUtc)
        {
            Id = id;
            ChatId = chatId;
            SenderId = senderId;
            Type = type;
            Text = text;
            CreatedAtUtc = createdAtUtc;
        }

        public static Result<ChatMessage> CreateText(
            Chat chat,
            Guid senderId,
            string text,
            DateTime createdAtUtc)
        {
            if (chat is null)
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.chat_required", "Chat is required"));
            }

            if (senderId == Guid.Empty)
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.sender_id_required", "SenderId is required"));
            }

            if (!chat.HasParticipant(senderId))
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.sender_not_participant", "Sender must be a chat participant"));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.text_required", "Text is required"));
            }

            var trimmedText = text.Trim();
            if (trimmedText.Length > MaxTextLength)
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.text_too_long", $"Text cannot exceed {MaxTextLength} characters"));
            }

            return Result<ChatMessage>.Success(
                new ChatMessage(
                    Guid.NewGuid(),
                    chat.Id,
                    senderId,
                    ChatMessageType.Text,
                    trimmedText,
                    createdAtUtc));
        }
    }
}
