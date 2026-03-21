namespace FitLead.IntegrationTests.Infrastructure;

public static class IntegrationTestCollectionNames
{
    public const string Default = "integration-tests";
}

[CollectionDefinition(IntegrationTestCollectionNames.Default)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
