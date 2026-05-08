using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Invitations.Queries;
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
                    Status = x.Status.ToString(),
                    CreatedAtUtc = x.CreatedAtUtc,
                    ExpiresAtUtc = x.ExpiresAtUtc,
                    AcceptedByClientId = x.AcceptedByClientId,
                    AcceptedAtUtc = x.AcceptedAtUtc
                })
                .ToListAsync(cancellationToken);
        }
    }
}
