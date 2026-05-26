using FitLead.Domain.Clients.ProgressPhotos;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IClientProgressPhotoRepository
    {
        Task AddAsync(
            ClientProgressPhoto photo,
            CancellationToken cancellationToken);

        Task<ClientProgressPhoto?> GetByIdForClientAsync(
            Guid photoId,
            Guid clientId,
            CancellationToken cancellationToken);

        void Remove(ClientProgressPhoto photo);
    }
}
