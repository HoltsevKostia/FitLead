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

    public async Task<ClientProfilesTestClient> ClientProfilesAsync(AuthTestClient auth)
    {
        var client = new ClientProfilesTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<BodyMetricsTestClient> BodyMetricsAsync(AuthTestClient auth)
    {
        var client = new BodyMetricsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<TrainerClientsTestClient> TrainerClientsAsync(AuthTestClient auth)
    {
        var client = new TrainerClientsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<ChatsTestClient> ChatsAsync(AuthTestClient auth)
    {
        var client = new ChatsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<MediaAssetsTestClient> MediaAssetsAsync(AuthTestClient auth)
    {
        var client = new MediaAssetsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<NotificationsTestClient> NotificationsAsync(AuthTestClient auth)
    {
        var client = new NotificationsTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    public async Task<PushTestClient> PushAsync(AuthTestClient auth)
    {
        var client = new PushTestClient(fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }
}
