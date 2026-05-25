using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public sealed record DeleteClientProgressPhotoCommand(Guid PhotoId)
        : IRequest<Result>;
}
