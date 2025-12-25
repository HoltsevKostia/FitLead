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
    public sealed class ExerciseRepository : IExerciseRepository
    {
        private readonly FitLeadDbContext _context;

        public ExerciseRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Exercise exercise,
            CancellationToken cancellationToken)
        {
            await _context.Exercises.AddAsync(exercise, cancellationToken);
        }

        public async Task<Exercise?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.Exercises
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken)
        {
            return await _context.Exercises
                .AnyAsync(x => x.Id == id, cancellationToken);
        }
    }
}
