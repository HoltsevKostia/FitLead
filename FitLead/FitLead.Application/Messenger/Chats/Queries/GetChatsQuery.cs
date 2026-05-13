using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.Chats.Queries
{
    public sealed record GetChatsQuery(

    ) : IRequest<Result<IReadOnlyList<ChatListItemDto>>>;
}
