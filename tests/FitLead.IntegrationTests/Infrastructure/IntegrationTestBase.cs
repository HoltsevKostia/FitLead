namespace FitLead.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollectionNames.Default)]
public abstract class IntegrationTestBase(IntegrationTestFixture fixture) : IAsyncLifetime
{
    protected IntegrationTestFixture Fixture { get; } = fixture;
    protected HttpClient HttpClient { get; } = fixture.CreateClient();
    protected static string UniqueEmail(string prefix = "user")
        => $"{prefix}-{Guid.NewGuid():N}@test.local";

    public virtual Task InitializeAsync() => Fixture.ResetDatabaseAsync();

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
