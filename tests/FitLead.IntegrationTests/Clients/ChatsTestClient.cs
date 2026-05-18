namespace FitLead.IntegrationTests.Clients;

public sealed class ChatsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "chats")
{
    public Task<HttpResponseMessage> GetChatsAsync(CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/chats", cancellationToken);
    }

    public Task<HttpResponseMessage> GetChatAsync(
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/chats/{chatId:D}", cancellationToken);
    }

    public Task<HttpResponseMessage> GetMessagesAsync(
        Guid chatId,
        int? limit = null,
        DateTime? beforeCreatedAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (limit.HasValue)
        {
            query.Add($"limit={limit.Value}");
        }

        if (beforeCreatedAtUtc.HasValue)
        {
            query.Add($"beforeCreatedAtUtc={Uri.EscapeDataString(beforeCreatedAtUtc.Value.ToString("O"))}");
        }

        var queryString = query.Count > 0
            ? "?" + string.Join("&", query)
            : string.Empty;

        return SendGetAsync($"/api/chats/{chatId:D}/messages{queryString}", cancellationToken);
    }

    public Task<HttpResponseMessage> GetOrCreateWithClientAsync(
        Guid clientId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/chats/with-client/{clientId:D}",
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> GetOrCreateWithTrainerAsync(
        Guid trainerId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/chats/with-trainer/{trainerId:D}",
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> SendTextMessageAsync(
        Guid chatId,
        string text,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/chats/{chatId:D}/messages",
            new { text },
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> CreateVideoReportAsync(
        Guid chatId,
        string title,
        IReadOnlyList<Guid> mediaAssetIds,
        string? description = null,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/chats/{chatId:D}/video-reports",
            new
            {
                title,
                description,
                mediaAssetIds
            },
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> GetVideoReportAsync(
        Guid chatId,
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync(
            $"/api/chats/{chatId:D}/video-reports/{reportId:D}",
            cancellationToken);
    }

    public Task<HttpResponseMessage> SubmitVideoReportFeedbackAsync(
        Guid chatId,
        Guid reportId,
        string text,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/chats/{chatId:D}/video-reports/{reportId:D}/feedback",
            new { text },
            cancellationToken,
            includeCsrfHeader);
    }
}
