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

    public async Task<TrainingProgramsTestClient> TrainingProgramsAsync(AuthTestClient auth)
    {
        var client = new TrainingProgramsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<ClientTrainingProgramsTestClient> ClientTrainingProgramsAsync(AuthTestClient auth)
    {
        var client = new ClientTrainingProgramsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<TrainerClientsTestClient> TrainerClientsAsync(AuthTestClient auth)
    {
        var client = new TrainerClientsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }
}
