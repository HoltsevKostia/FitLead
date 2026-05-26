namespace FitLead.IntegrationTests.Clients;

public sealed class PushTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "push")
{
    public Task<HttpResponseMessage> GetVapidPublicKeyAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/push/vapid-public-key", cancellationToken);
    }

    public Task<HttpResponseMessage> RegisterSubscriptionAsync(
        string endpoint,
        string p256dh = "test-p256dh",
        string auth = "test-auth",
        string? userAgent = "Test browser",
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/push/subscriptions",
            new
            {
                endpoint,
                keys = new
                {
                    p256dh,
                    auth
                },
                userAgent
            },
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> RevokeCurrentSubscriptionAsync(
        string endpoint,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/push/subscriptions/current/revoke",
            new
            {
                endpoint
            },
            cancellationToken,
            includeCsrfHeader);
    }
}
