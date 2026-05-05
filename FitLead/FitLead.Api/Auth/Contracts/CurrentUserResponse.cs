namespace FitLead.Api.Auth.Contracts
{
    public sealed record CurrentUserResponse(
        string Id,
        string Email,
        string Role);
}
