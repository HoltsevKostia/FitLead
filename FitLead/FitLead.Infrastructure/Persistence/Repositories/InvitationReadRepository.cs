using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Invitations.Queries;
using FitLead.Domain.Invitations;
using Microsoft.EntityFrameworkCore;


namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class InvitationReadRepository
    : IInvitationReadRepository
    {
        private readonly FitLeadDbContext _context;

        public InvitationReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<InvitationDto>> GetPendingForClientAsync(
            Guid clientId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            return await _context.Invitations
                .Where(x =>
                    x.ClientId == clientId &&
                    x.Status == InvitationStatus.Pending &&
                    x.ExpiresAt > now)
                .Select(x => new InvitationDto
                {
                    Id = x.Id,
                    TrainerId = x.TrainerId,
                    ClientId = x.ClientId,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    ExpiresAt = x.ExpiresAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<InvitationDto>> GetSentByTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await _context.Invitations
                .Where(x => x.TrainerId == trainerId)
                .Select(x => new InvitationDto
                {
                    Id = x.Id,
                    TrainerId = x.TrainerId,
                    ClientId = x.ClientId,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    ExpiresAt = x.ExpiresAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
