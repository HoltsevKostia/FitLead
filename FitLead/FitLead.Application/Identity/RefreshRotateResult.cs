namespace FitLead.Application.Identity
{
    public enum RefreshRotateFailure
    {
        None = 0,
        Invalid = 1,
        Expired = 2,
        ReuseDetected = 3,
        Revoked = 4,
        UserUnavailable = 5
    }

    public sealed record RefreshRotateResult(
        bool Success,
        string? IdentityUserId,
        string? NewRefreshToken,
        RefreshRotateFailure Failure)
    {
        public static RefreshRotateResult Succeeded(
            string identityUserId,
            string newRefreshToken) =>
            new(true, identityUserId, newRefreshToken, RefreshRotateFailure.None);

        public static RefreshRotateResult Failed(RefreshRotateFailure failure) =>
            new(false, null, null, failure);
    }
}
