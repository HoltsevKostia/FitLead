using FitLead.Application.Abstractions.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FitLeadDbContext _context;

        public UnitOfWork(FitLeadDbContext context)
        {
            _context = context;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
