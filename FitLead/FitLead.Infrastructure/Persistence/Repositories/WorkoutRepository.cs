using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Trainings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    internal sealed class WorkoutRepository : IWorkoutRepository
    {
        private readonly FitLeadDbContext _context;

        public WorkoutRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<Workout?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.Workouts
                .Include(x => x.Exercises)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(
            Workout workout,
            CancellationToken cancellationToken)
        {
            await _context.Workouts.AddAsync(workout, cancellationToken);
        }

        public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken)
        {
            return await _context.Workouts
                .AnyAsync(x => x.Id == id, cancellationToken);
        }
    }
}
