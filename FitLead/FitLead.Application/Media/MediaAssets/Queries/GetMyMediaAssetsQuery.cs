using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Media.MediaAssets.Queries
{
    public sealed record GetMyMediaAssetsQuery()
        : IRequest<Result<IReadOnlyList<MediaAssetDto>>>;
}
