using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public sealed record GetClientProgressPhotosQuery()
        : IRequest<Result<IReadOnlyList<ClientProgressPhotoDto>>>;
}
