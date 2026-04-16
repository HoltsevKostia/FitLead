namespace FitLead.Api.Auth.Contracts
{
    public sealed record LogoutRequest(
        string RefreshToken);
}
