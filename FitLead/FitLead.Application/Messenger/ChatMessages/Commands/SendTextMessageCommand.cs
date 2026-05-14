using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.ChatMessages.Commands
{
    public sealed record SendTextMessageCommand(
        Guid ChatId,
        string Text
    ) : IRequest<Result<ChatMessageDto>>;
}
