namespace FitLead.Api.Contracts.Auth
{
    public sealed record LoginResponse(
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);
}
