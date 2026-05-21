namespace FitLead.IntegrationTests.Clients;

public sealed class NotificationsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "notifications")
{
    public Task<HttpResponseMessage> GetNotificationsAsync(
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = limit.HasValue
            ? $"?limit={limit.Value}"
            : string.Empty;

        return SendGetAsync($"/api/notifications{query}", cancellationToken);
    }

    public Task<HttpResponseMessage> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/notifications/unread-count", cancellationToken);
    }

    public Task<HttpResponseMessage> MarkReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/notifications/{notificationId:D}/read",
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> MarkAllReadAsync(
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            "/api/notifications/read-all",
            cancellationToken,
            includeCsrfHeader);
    }
}
