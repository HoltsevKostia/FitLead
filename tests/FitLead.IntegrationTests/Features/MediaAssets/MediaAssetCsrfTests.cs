using System.Net;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.MediaAssets;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class MediaAssetCsrfTests : MediaAssetTestBase
{
    public MediaAssetCsrfTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Register_WithoutCsrf_ShouldReturnBadRequest()
    {
        var user = await Users.RegisterTrainerAsync("media-csrf");
        var mediaAssets = await Api.MediaAssetsAsync(user.Auth);

        var response = await mediaAssets.RegisterAsync(includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
