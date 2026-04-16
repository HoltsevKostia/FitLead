namespace FitLead.Api.Auth.Contracts
{
    public sealed record RefreshResponse(
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);
}
