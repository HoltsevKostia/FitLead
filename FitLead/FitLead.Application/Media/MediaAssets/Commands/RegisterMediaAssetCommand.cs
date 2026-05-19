using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Media.MediaAssets.Commands
{
    public sealed record RegisterMediaAssetCommand(
        string StorageProvider,
        string StorageObjectId,
        string DeliveryUrl,
        string? FileName,
        string ContentType,
        long SizeBytes,
        string Kind,
        int? DurationSeconds
    ) : IRequest<Result<MediaAssetDto>>;
}
