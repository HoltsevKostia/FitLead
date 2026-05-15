using FitLead.Application.Messenger.Chats.Queries;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.Chats.Commands
{
    public sealed record GetOrCreateChatWithTrainerCommand(
        Guid TrainerId
    ) : IRequest<Result<ChatDto>>;
}
