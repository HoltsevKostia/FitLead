using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Clients.ProgressPhotos;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ClientProgressPhotoRepository : IClientProgressPhotoRepository
    {
        private readonly FitLeadDbContext _context;

        public ClientProgressPhotoRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            ClientProgressPhoto photo,
            CancellationToken cancellationToken)
        {
            await _context.ClientProgressPhotos.AddAsync(photo, cancellationToken);
        }

        public async Task<ClientProgressPhoto?> GetByIdForClientAsync(
            Guid photoId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.ClientProgressPhotos
                .FirstOrDefaultAsync(
                    photo => photo.Id == photoId &&
                             photo.ClientId == clientId,
                    cancellationToken);
        }

        public void Remove(ClientProgressPhoto photo)
        {
            _context.ClientProgressPhotos.Remove(photo);
        }
    }
}
