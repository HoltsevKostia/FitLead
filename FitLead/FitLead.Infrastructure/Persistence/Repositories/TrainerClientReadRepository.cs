using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientReadRepository
    : ITrainerClientReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainerClientDto>> GetClientsByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await (
                from tc in _context.TrainerClients
                join u in _context.DomainUsers on tc.ClientId equals u.Id
                where tc.TrainerId == trainerId
                select new TrainerClientDto
                {
                    ClientId = u.Id,
                    Email = u.Email,
                    FullName = u.FullName
                }
            ).ToListAsync(cancellationToken);
        }
        public async Task<IReadOnlyList<TrainerClientOverviewDto>> GetClientsOverviewByTrainerIdAsync(
            Guid trainerId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            var clients = await (
                from trainerClient in _context.TrainerClients.AsNoTracking()
                join client in _context.DomainUsers.AsNoTracking()
                    on trainerClient.ClientId equals client.Id
                where trainerClient.TrainerId == trainerId
                orderby client.FullName
                select new
                {
                    ClientId = client.Id,
                    client.Email,
                    client.FullName
                })
                .ToListAsync(cancellationToken);

            var activePrograms = await (
                from assignment in _context.AssignedTrainingPrograms.AsNoTracking()
                join program in _context.TrainingPrograms.AsNoTracking()
                    on assignment.TrainingProgramId equals program.Id
                where assignment.TrainerId == trainerId &&
                      assignment.Status == AssignedProgramStatus.Active &&
                      (!assignment.ExpiresAtUtc.HasValue || assignment.ExpiresAtUtc > utcNow)
                orderby assignment.AssignedAtUtc descending
                select new
                {
                    assignment.ClientId,
                    Access = new TrainerClientProgramAccessDto(
                        assignment.Id,
                        program.Id,
                        program.Title,
                        assignment.AssignedAtUtc,
                        assignment.ExpiresAtUtc)
                })
                .ToListAsync(cancellationToken);

            var activeProgramsByClient = activePrograms
                .GroupBy(program => program.ClientId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<TrainerClientProgramAccessDto>)group
                        .Select(program => program.Access)
                        .ToList());

            return clients
                .Select(client => new TrainerClientOverviewDto(
                    client.ClientId,
                    client.Email,
                    client.FullName,
                    activeProgramsByClient.TryGetValue(client.ClientId, out var programs)
                        ? programs
                        : Array.Empty<TrainerClientProgramAccessDto>()))
                .ToList();
        }
    }
}
