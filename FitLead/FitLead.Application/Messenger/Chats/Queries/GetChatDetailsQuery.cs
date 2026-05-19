using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed record GetChatDetailsQuery(
        Guid ChatId
    ) : IRequest<Result<ChatDetailsDto>>;
}
