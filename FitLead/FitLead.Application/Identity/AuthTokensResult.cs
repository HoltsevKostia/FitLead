namespace FitLead.Application.Identity
{
    public sealed record AuthTokensResult(
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);
}
