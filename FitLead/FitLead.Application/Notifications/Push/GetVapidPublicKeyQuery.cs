using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Push
{
    public sealed record GetVapidPublicKeyQuery()
        : IRequest<Result<VapidPublicKeyDto>>;
}
