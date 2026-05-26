using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;

namespace FitLead.IntegrationTests.Features.ClientProfiles;

public abstract class ClientProfileTestBase : IntegrationTestBase
{
    protected readonly TestDb Db;
    protected readonly TestUsers Users;
    protected readonly TestApiClients Api;

    protected ClientProfileTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Users = new TestUsers(fixture, Db);
        Api = new TestApiClients(fixture);
    }
}
