using FitLead.Application.Clients.ProgressPhotos;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IClientProgressPhotoReadRepository
    {
        Task<IReadOnlyList<ClientProgressPhotoDto>> GetByClientAsync(
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
