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
}
