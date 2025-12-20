using FitLead.Application.Users.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientReadRepository
    {
        Task<IReadOnlyList<TrainerClientDto>> GetClientsByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
