using System.Net.Http.Json;
using FitLead.Api.Invitations.Contracts;

namespace FitLead.IntegrationTests.Clients;

public sealed class InvitationsTestClient(HttpClient httpClient)
{
    public Task<HttpResponseMessage> CreateAsync(
        int expiresInDays,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateInvitationRequest(expiresInDays);
        return httpClient.PostAsJsonAsync("/api/invitations", request, cancellationToken);
    }

    public Task<HttpResponseMessage> GetTrainerInvitationsAsync(
        CancellationToken cancellationToken = default)
    {
        return httpClient.GetAsync("/api/invitations/trainer", cancellationToken);
    }

    public Task<HttpResponseMessage> PreviewAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return httpClient.GetAsync($"/api/invitations/{Uri.EscapeDataString(token)}/preview", cancellationToken);
    }

    public Task<HttpResponseMessage> AcceptAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return httpClient.PostAsync($"/api/invitations/{Uri.EscapeDataString(token)}/accept", content: null, cancellationToken);
    }

    public Task<HttpResponseMessage> RevokeAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        return httpClient.PostAsync($"/api/invitations/{invitationId:D}/revoke", content: null, cancellationToken);
    }
}
