namespace FitLead.Api.Auth.Contracts
{
    public sealed record LoginResponse(
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);
}
