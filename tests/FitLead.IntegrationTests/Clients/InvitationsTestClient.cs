using FitLead.Api.Invitations.Contracts;

namespace FitLead.IntegrationTests.Clients;

public sealed class InvitationsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "invitations")
{
    public Task<HttpResponseMessage> CreateAsync(
        int expiresInDays,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/invitations",
            new CreateInvitationRequest(expiresInDays),
            cancellationToken);
    }

    public Task<HttpResponseMessage> CreateWithoutCsrfAsync(
        int expiresInDays,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/invitations",
            new CreateInvitationRequest(expiresInDays),
            cancellationToken,
            includeCsrfHeader: false);
    }

    public Task<HttpResponseMessage> GetTrainerInvitationsAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/invitations/trainer", cancellationToken);
    }

    public Task<HttpResponseMessage> PreviewAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/invitations/{Uri.EscapeDataString(token)}/preview", cancellationToken);
    }

    public Task<HttpResponseMessage> AcceptAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/invitations/{Uri.EscapeDataString(token)}/accept",
            cancellationToken);
    }

    public Task<HttpResponseMessage> AcceptWithoutCsrfAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/invitations/{Uri.EscapeDataString(token)}/accept",
            cancellationToken,
            includeCsrfHeader: false);
    }

    public Task<HttpResponseMessage> RevokeAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/invitations/{invitationId:D}/revoke",
            cancellationToken);
    }

    public Task<HttpResponseMessage> RevokeWithoutCsrfAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/invitations/{invitationId:D}/revoke",
            cancellationToken,
            includeCsrfHeader: false);
    }
}
