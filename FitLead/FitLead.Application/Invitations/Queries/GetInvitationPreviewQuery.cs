using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Queries
{
    public sealed record GetInvitationPreviewQuery(
        string Token
    ) : IRequest<Result<InvitationPreviewDto>>;
}
