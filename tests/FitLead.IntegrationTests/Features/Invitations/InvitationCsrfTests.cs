using System.Net;
using FitLead.Application.Invitations.Commands;
using FitLead.Infrastructure.Persistence;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Invitations;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class InvitationCsrfTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-create-trainer"),
            "Str0ngPass!123",
            "Create Csrf Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var response = await trainerInvitations.CreateWithoutCsrfAsync(7);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithValidCsrf_ShouldFollowExistingSuccessContract()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(trainerHttp);

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-create-success"),
            "Str0ngPass!123",
            "Create Success Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var response = await trainerInvitations.CreateAsync(7);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.ReadRequiredJsonAsync<CreateInvitationResult>();
        payload.InvitationId.Should().NotBeEmpty();
        payload.InviteUrl.Should().StartWith("http://localhost:3000/invite/");
        payload.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow.AddDays(6));
    }

    [Fact]
    public async Task Preview_WithoutCsrf_ShouldRemainAllowed()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(trainerHttp);
        var anonymousInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-preview-trainer"),
            "Str0ngPass!123",
            "Preview Csrf Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var response = await anonymousInvitations.PreviewAsync(ExtractToken(created.InviteUrl));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Accept_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var clientHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(trainerHttp);
        var clientAuth = new AuthTestClient(clientHttp);
        var clientInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-accept-trainer"),
            "Str0ngPass!123",
            "Accept Csrf Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await clientAuth.RegisterAsync(
            UniqueEmail("inv-csrf-accept-client"),
            "Str0ngPass!123",
            "Accept Csrf Client",
            AuthRoles.Client)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        await clientInvitations.CopyAuthStateFromAsync(clientAuth);

        var response = await clientInvitations.AcceptWithoutCsrfAsync(ExtractToken(created.InviteUrl));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Accept_WithValidCsrf_ShouldFollowExistingSuccessContract()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var clientHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(trainerHttp);
        var clientAuth = new AuthTestClient(clientHttp);
        var clientInvitations = new InvitationsTestClient(clientHttp);

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-accept-success-trainer"),
            "Str0ngPass!123",
            "Accept Success Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await clientAuth.RegisterAsync(
            UniqueEmail("inv-csrf-accept-success-client"),
            "Str0ngPass!123",
            "Accept Success Client",
            AuthRoles.Client)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);
        await clientInvitations.CopyAuthStateFromAsync(clientAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var response = await clientInvitations.AcceptAsync(ExtractToken(created.InviteUrl));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var relationships = await db.TrainerClients.ToListAsync();
        relationships.Should().ContainSingle();
    }

    [Fact]
    public async Task TrainerInvitationsGet_WithoutCsrf_ShouldRemainAllowedForAuthenticatedTrainer()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(trainerHttp);

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-get-trainer"),
            "Str0ngPass!123",
            "Get Csrf Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var response = await trainerInvitations.GetTrainerInvitationsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Revoke_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(trainerHttp);
        var trainerNoCsrfInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-revoke-trainer"),
            "Str0ngPass!123",
            "Revoke Csrf Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        await trainerNoCsrfInvitations.CopyAuthStateFromAsync(trainerAuth);

        var response = await trainerNoCsrfInvitations.RevokeWithoutCsrfAsync(created.InvitationId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Revoke_WithValidCsrf_ShouldFollowExistingSuccessContract()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(trainerHttp);

        (await trainerAuth.RegisterAsync(
            UniqueEmail("inv-csrf-revoke-success-trainer"),
            "Str0ngPass!123",
            "Revoke Success Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var response = await trainerInvitations.RevokeAsync(created.InvitationId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static string ExtractToken(string inviteUrl)
    {
        var uri = new Uri(inviteUrl, UriKind.Absolute);
        return uri.Segments[^1].Trim('/');
    }
}
