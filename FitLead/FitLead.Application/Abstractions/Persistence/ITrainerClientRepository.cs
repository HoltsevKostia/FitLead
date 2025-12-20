using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientRepository
    {
        Task<bool> ExistsAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);

        Task AddAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
