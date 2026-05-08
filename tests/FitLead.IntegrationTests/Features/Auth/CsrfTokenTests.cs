using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Auth;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class CsrfTokenTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCsrfToken_Anonymous_ShouldReturnNoContent()
    {
        var response = await HttpClient.GetAsync("/auth/csrf-token");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetCsrfToken_ShouldSetReadableXsrfTokenCookie()
    {
        var response = await HttpClient.GetAsync("/auth/csrf-token");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cookie = response.GetRequiredCookie(ApiCsrfTokenNames.RequestTokenCookie);
        cookie.Value.Should().NotBeNullOrWhiteSpace();
        cookie.HttpOnly.Should().BeFalse();
        cookie.Path.Should().Be("/");
    }

    [Fact]
    public async Task GetCsrfToken_ShouldSetInternalAntiforgeryCookie()
    {
        var response = await HttpClient.GetAsync("/auth/csrf-token");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cookie = response.GetRequiredCookie(ApiCsrfTokenNames.AntiforgeryCookie);
        cookie.Value.Should().NotBeNullOrWhiteSpace();
        cookie.HttpOnly.Should().BeTrue();
        cookie.Path.Should().Be("/");
    }

    [Fact]
    public async Task GetCsrfToken_ShouldNotRequireAuthentication()
    {
        using var anonymousClient = Fixture.CreateClient();

        var response = await anonymousClient.GetAsync("/auth/csrf-token");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
