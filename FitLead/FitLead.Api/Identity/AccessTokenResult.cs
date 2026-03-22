namespace FitLead.Api.Identity
{
    public sealed record AccessTokenResult(
        string AccessToken,
        int ExpiresIn);
}
