namespace FitLead.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollectionNames.Default)]
public abstract class IntegrationTestBase(IntegrationTestFixture fixture) : IAsyncLifetime
{
    protected HttpClient HttpClient { get; } = fixture.CreateClient();

    public virtual Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
