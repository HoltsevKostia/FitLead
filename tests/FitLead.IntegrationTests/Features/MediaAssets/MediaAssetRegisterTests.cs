using System.Net;
using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.MediaAssets;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class MediaAssetRegisterTests : MediaAssetTestBase
{
    public MediaAssetRegisterTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Register_WithInvalidProvider_ShouldReturnValidationError()
    {
        var user = await Users.RegisterTrainerAsync("media-invalid-provider");
        var mediaAssets = await Api.MediaAssetsAsync(user.Auth);

        var response = await mediaAssets.RegisterAsync(storageProvider: "Unknown");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("media_asset.storage_provider_invalid");
    }

    [Fact]
    public async Task Register_WithInvalidKind_ShouldReturnValidationError()
    {
        var user = await Users.RegisterTrainerAsync("media-invalid-kind");
        var mediaAssets = await Api.MediaAssetsAsync(user.Auth);

        var response = await mediaAssets.RegisterAsync(kind: "Document");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("media_asset.kind_invalid");
    }

    [Fact]
    public async Task Register_DuplicateForSameOwner_ShouldReturnExistingAsset()
    {
        var user = await Users.RegisterTrainerAsync("media-same-owner");
        var mediaAssets = await Api.MediaAssetsAsync(user.Auth);

        var firstResponse = await mediaAssets.RegisterAsync(storageObjectId: "same-owner-object");
        var secondResponse = await mediaAssets.RegisterAsync(storageObjectId: "same-owner-object");

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var firstAsset = await firstResponse.ReadRequiredJsonAsync<MediaAssetDto>();
        var secondAsset = await secondResponse.ReadRequiredJsonAsync<MediaAssetDto>();
        secondAsset.Id.Should().Be(firstAsset.Id);
    }

    [Fact]
    public async Task Register_DuplicateForDifferentOwner_ShouldReturnConflict()
    {
        var firstUser = await Users.RegisterTrainerAsync("media-first-owner");
        var secondUser = await Users.RegisterTrainerAsync("media-second-owner");
        var firstMediaAssets = await Api.MediaAssetsAsync(firstUser.Auth);
        var secondMediaAssets = await Api.MediaAssetsAsync(secondUser.Auth);

        var firstResponse = await firstMediaAssets.RegisterAsync(storageObjectId: "shared-object");
        var secondResponse = await secondMediaAssets.RegisterAsync(storageObjectId: "shared-object");

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await secondResponse.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("media_asset.storage_object_already_registered");
    }
}
