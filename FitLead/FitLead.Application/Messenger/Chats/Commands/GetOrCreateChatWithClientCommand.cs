using FitLead.Application.Messenger.Chats.Queries;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.Chats.Commands
{
    public sealed record GetOrCreateChatWithClientCommand(
        Guid ClientId
    ) : IRequest<Result<ChatDto>>;
}
