using FitLead.Common.Results;

namespace FitLead.Application.Identity
{
    public interface IIdentityAccountService
    {
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

        Task<Result<IdentityProvisionResult>> CreateWithRoleAsync(
            string email,
            string password,
            string role,
            CancellationToken cancellationToken);
    }
}
