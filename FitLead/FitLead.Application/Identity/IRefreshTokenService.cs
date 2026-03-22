namespace FitLead.Application.Identity
{
    public interface IRefreshTokenService
    {
        Task<(string RefreshToken, Guid FamilyId)> IssueForLoginAsync(
            string identityUserId,
            CancellationToken cancellationToken);

        Task<RefreshRotateResult> RotateAsync(
            string refreshToken,
            CancellationToken cancellationToken);

        Task RevokeFamilyByTokenAsync(
            string refreshToken,
            string reason,
            CancellationToken cancellationToken);
    }
}
