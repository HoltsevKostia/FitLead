namespace FitLead.Api.Auth.Contracts
{
    public sealed record RegisterRequest(
        string Email,
        string Password,
        string FullName,
        string Role);
}
