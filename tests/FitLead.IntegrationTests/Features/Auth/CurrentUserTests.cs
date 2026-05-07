using System.Net;
using System.Text.Json;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class CurrentUserTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CurrentUser_WithValidAuthCookies_ShouldReturnIdEmailAndRole()
    {
        var authClient = new AuthTestClient(HttpClient);
        var email = UniqueEmail("claims");

        var register = await authClient.RegisterAsync(email, "Str0ngPass!123", "Claims User", AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await HttpClient.GetAsync("/auth/current-user");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("id").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("email").GetString().Should().Be(email);
        json.RootElement.GetProperty("role").GetString().Should().Be(AuthRoles.Trainer);
    }
}
