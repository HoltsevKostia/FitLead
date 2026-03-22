namespace FitLead.Api.Contracts.Auth
{
    public sealed record RegisterRequest(
        string Email,
        string Password,
        string FullName,
        string Role);
}
