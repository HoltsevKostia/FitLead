namespace FitLead.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _databaseFixture = new();

    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    public HttpClient CreateClient() => Factory.CreateClient();

    public Task ResetDatabaseAsync() => Factory.ResetDatabaseAsync();

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        Factory = new CustomWebApplicationFactory(_databaseFixture.ConnectionString);
        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
            Factory.Dispose();

        await _databaseFixture.DisposeAsync();
    }
}
