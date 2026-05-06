using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Invitations;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    internal sealed class InvitationRepository : IInvitationRepository
    {
        private readonly FitLeadDbContext _context;

        public InvitationRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Invitation invitation,
            CancellationToken cancellationToken)
        {
            await _context.Invitations.AddAsync(invitation, cancellationToken);
        }

        public async Task<Invitation?> GetByIdAsync(
            Guid invitationId,
            CancellationToken cancellationToken)
        {
            return await _context.Invitations
                .FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);
        }

        public async Task<Invitation?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken)
        {
            return await _context.Invitations
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        }
    }
}
