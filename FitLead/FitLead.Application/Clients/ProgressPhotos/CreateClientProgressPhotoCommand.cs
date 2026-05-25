using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public sealed record CreateClientProgressPhotoCommand(
        Guid MediaAssetId,
        DateOnly TakenAt,
        string? Label,
        string? Note)
        : IRequest<Result<ClientProgressPhotoDto>>;
}
