using System.Net;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Helpers;

public sealed class TestUsers(IntegrationTestFixture fixture, TestDb db)
{
    public async Task<TestUser> RegisterTrainerAsync(string prefix)
    {
        return await RegisterAsync(prefix, "Test Trainer", AuthRoles.Trainer);
    }

    public async Task<TestUser> RegisterClientAsync(string prefix)
    {
        return await RegisterAsync(prefix, "Test Client", AuthRoles.Client);
    }

    private async Task<TestUser> RegisterAsync(
        string prefix,
        string fullName,
        string role)
    {
        var auth = new AuthTestClient(fixture.CreateClient(handleCookies: false));
        var email = $"{prefix}-{Guid.NewGuid():N}@test.local";

        var response = await auth.RegisterAsync(
            email,
            "Str0ngPass!123",
            fullName,
            role);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var userId = await db.QueryAsync(context =>
            context.DomainUsers
                .Where(x => x.Email == email)
                .Select(x => x.Id)
                .SingleAsync());

        return new TestUser(auth, userId, email);
    }
}
