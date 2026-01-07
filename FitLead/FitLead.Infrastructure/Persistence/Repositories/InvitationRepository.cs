using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Invitations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<bool> ExistsPendingAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.Invitations.AnyAsync(
                x => x.TrainerId == trainerId
                  && x.ClientId == clientId
                  && x.Status == InvitationStatus.Pending,
                cancellationToken);
        }

        public async Task<int> CountSentByTrainerForDateAsync(
            Guid trainerId,
            DateTime dateUtc,
            CancellationToken cancellationToken)
        {
            var start = dateUtc.Date;
            var end = start.AddDays(1);

            return await _context.Invitations.CountAsync(
                x => x.TrainerId == trainerId
                  && x.CreatedAt >= start
                  && x.CreatedAt < end,
                cancellationToken);
        }

        public async Task<IReadOnlyList<Invitation>> GetExpiredPendingAsync(DateTime now, CancellationToken cancellationToken)
        {
            return await _context.Invitations
             .Where(i => i.Status == InvitationStatus.Pending)
             .Where(i => i.ExpiresAt <= now)                  
             .ToListAsync(cancellationToken);
        }
    }
}
