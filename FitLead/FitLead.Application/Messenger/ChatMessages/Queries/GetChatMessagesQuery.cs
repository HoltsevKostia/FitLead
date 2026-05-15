using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.ChatMessages.Queries
{
    public sealed record GetChatMessagesQuery(
        Guid ChatId,
        int? Limit,
        DateTime? BeforeCreatedAtUtc
    ) : IRequest<Result<ChatMessageHistoryDto>>;
}
