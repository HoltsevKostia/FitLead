using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;

namespace FitLead.Domain.Messenger.ChatMessages
{
    public sealed class ChatMessage : Entity<Guid>
    {
        public const int MaxTextLength = 4000;

        public Guid ChatId { get; private set; }
        public Guid SenderId { get; private set; }
        public ChatMessageType Type { get; private set; }
        public string? Text { get; private set; }
        public Guid? VideoReportId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        private ChatMessage() { }

        private ChatMessage(
            Guid id,
            Guid chatId,
            Guid senderId,
            ChatMessageType type,
            string? text,
            Guid? videoReportId,
            DateTime createdAtUtc)
        {
            Id = id;
            ChatId = chatId;
            SenderId = senderId;
            Type = type;
            Text = text;
            VideoReportId = videoReportId;
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
                    null,
                    createdAtUtc));
        }

        public static Result<ChatMessage> CreateVideoReport(
            Chat chat,
            VideoReport videoReport,
            Guid senderId,
            DateTime createdAtUtc)
        {
            if (chat is null)
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.chat_required", "Chat is required"));
            }

            if (videoReport is null)
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.video_report_required", "VideoReport is required"));
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

            if (videoReport.ChatId != chat.Id)
            {
                return Result<ChatMessage>.Failure(
                    Error.Validation("chat_message.create.video_report_chat_mismatch", "VideoReport must belong to the chat"));
            }

            return Result<ChatMessage>.Success(
                new ChatMessage(
                    Guid.NewGuid(),
                    chat.Id,
                    senderId,
                    ChatMessageType.VideoReport,
                    null,
                    videoReport.Id,
                    createdAtUtc));
        }
    }
}
