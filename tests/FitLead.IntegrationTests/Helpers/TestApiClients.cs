using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Infrastructure;

namespace FitLead.IntegrationTests.Helpers;

public sealed class TestApiClients(IntegrationTestFixture fixture)
{
    public async Task<ExercisesTestClient> ExercisesAsync(AuthTestClient auth)
    {
        var client = new ExercisesTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<WorkoutsTestClient> WorkoutsAsync(AuthTestClient auth)
    {
        var client = new WorkoutsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }
}
