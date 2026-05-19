using FitLead.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace FitLead.IntegrationTests.Clients;

public sealed class ChatHubTestClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    private ChatHubTestClient(HubConnection connection)
    {
        _connection = connection;
    }

    public static async Task<ChatHubTestClient> ConnectAsync(
        IntegrationTestFixture fixture,
        AuthTestClient auth,
        CancellationToken cancellationToken = default)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(
                "http://localhost/hubs/chat",
                options =>
                {
                    options.Headers["Cookie"] = auth.GetCookieHeader();
                    options.HttpMessageHandlerFactory = _ => fixture.Factory.Server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                })
            .Build();

        await connection.StartAsync(cancellationToken);

        return new ChatHubTestClient(connection);
    }

    public Task JoinChatAsync(
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        return _connection.InvokeAsync("JoinChat", chatId, cancellationToken);
    }

    public IDisposable OnMessageCreated<TPayload>(Action<TPayload> handler)
    {
        return _connection.On("MessageCreated", handler);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return _connection.StopAsync(cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _connection.StartAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}
