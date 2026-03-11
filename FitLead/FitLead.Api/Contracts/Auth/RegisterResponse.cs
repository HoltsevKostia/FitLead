namespace FitLead.Api.Contracts.Auth
{
    public sealed record RegisterResponse(
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);
}
