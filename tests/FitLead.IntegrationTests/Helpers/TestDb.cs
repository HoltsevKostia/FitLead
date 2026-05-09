using FitLead.Infrastructure.Persistence;
using FitLead.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Helpers;

public sealed class TestDb(IntegrationTestFixture fixture)
{
    public async Task ExecuteAsync(Func<FitLeadDbContext, Task> action)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        await action(dbContext);
    }

    public async Task<T> QueryAsync<T>(Func<FitLeadDbContext, Task<T>> query)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        return await query(dbContext);
    }
}
