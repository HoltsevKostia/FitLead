using FitLead.Common.Results;
using FitLead.Domain.Users;

namespace FitLead.Application.Users.Access
{
    public interface ICurrentUserLoader
    {
        Task<Result<User>> GetCurrentOrNotFoundAsync(CancellationToken cancellationToken);
    }
}
