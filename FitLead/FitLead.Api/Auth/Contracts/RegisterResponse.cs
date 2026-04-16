namespace FitLead.Api.Auth.Contracts
{
    public sealed record RegisterResponse(
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);
}
