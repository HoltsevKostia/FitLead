using FitLead.Common.Results;

namespace FitLead.Application.Identity
{
    public interface IAuthTokenIssuer
    {
        Task<Result<AuthTokensResult>> IssueAsync(
            string identityUserId,
            string businessRole,
            CancellationToken cancellationToken);
    }
}
