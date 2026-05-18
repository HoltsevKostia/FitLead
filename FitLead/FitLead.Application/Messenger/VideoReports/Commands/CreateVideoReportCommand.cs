using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Messenger.VideoReports.Commands
{
    public sealed record CreateVideoReportCommand(
        Guid ChatId,
        string Title,
        string? Description,
        IReadOnlyList<Guid> MediaAssetIds)
        : IRequest<Result<ChatMessageDto>>;
}
