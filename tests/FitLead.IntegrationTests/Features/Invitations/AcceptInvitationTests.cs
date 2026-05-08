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
public sealed class AcceptInvitationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Accept_WithValidClient_ShouldReturnNoContentAndCreateRelationship()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var clientHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var clientAuth = new AuthTestClient(clientHttp);
        var clientInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("accept-trainer"),
            "Str0ngPass!123",
            "Accept Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        var clientEmail = UniqueEmail("accept-client");
        (await clientAuth.RegisterAsync(
            clientEmail,
            "Str0ngPass!123",
            "Accept Client",
            AuthRoles.Client)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);
        await clientInvitations.CopyAuthStateFromAsync(clientAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var response = await clientInvitations.AcceptAsync(ExtractToken(created.InviteUrl));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var invitation = await db.Invitations.SingleAsync();
        invitation.Status.Should().Be(FitLead.Domain.Invitations.InvitationStatus.Accepted);
        invitation.AcceptedByClientId.Should().NotBeNull();
        invitation.AcceptedAtUtc.Should().NotBeNull();

        var relationships = await db.TrainerClients.ToListAsync();
        relationships.Should().ContainSingle();
        var relationship = relationships.Single();
        relationship.TrainerId.Should().Be(invitation.TrainerId);
        relationship.ClientId.Should().Be(invitation.AcceptedByClientId!.Value);
    }

    [Fact]
    public async Task Accept_WithSameClientTwice_ShouldBeIdempotentAndNotDuplicateRelationship()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var clientHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var clientAuth = new AuthTestClient(clientHttp);
        var clientInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("idempotent-trainer"),
            "Str0ngPass!123",
            "Idempotent Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        (await clientAuth.RegisterAsync(
            UniqueEmail("idempotent-client"),
            "Str0ngPass!123",
            "Idempotent Client",
            AuthRoles.Client)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);
        await clientInvitations.CopyAuthStateFromAsync(clientAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();
        var token = ExtractToken(created.InviteUrl);

        (await clientInvitations.AcceptAsync(token)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await clientInvitations.AcceptAsync(token)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var relationships = await db.TrainerClients.ToListAsync();
        relationships.Should().ContainSingle();
    }

    [Fact]
    public async Task Accept_WithClientThatAlreadyHasAnotherTrainer_ShouldReturnConflict()
    {
        var firstTrainerHttp = Fixture.CreateClient(handleCookies: false);
        var secondTrainerHttp = Fixture.CreateClient(handleCookies: false);
        var clientHttp = Fixture.CreateClient(handleCookies: false);

        var firstTrainerAuth = new AuthTestClient(firstTrainerHttp);
        var firstTrainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var secondTrainerAuth = new AuthTestClient(secondTrainerHttp);
        var secondTrainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var clientAuth = new AuthTestClient(clientHttp);
        var clientInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await firstTrainerAuth.RegisterAsync(
            UniqueEmail("first-trainer"),
            "Str0ngPass!123",
            "First Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await secondTrainerAuth.RegisterAsync(
            UniqueEmail("second-trainer"),
            "Str0ngPass!123",
            "Second Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await clientAuth.RegisterAsync(
            UniqueEmail("conflict-client"),
            "Str0ngPass!123",
            "Conflict Client",
            AuthRoles.Client)).StatusCode.Should().Be(HttpStatusCode.Created);

        await firstTrainerInvitations.CopyAuthStateFromAsync(firstTrainerAuth);
        await secondTrainerInvitations.CopyAuthStateFromAsync(secondTrainerAuth);
        await clientInvitations.CopyAuthStateFromAsync(clientAuth);

        var firstInvite = await firstTrainerInvitations.CreateAsync(7);
        var firstCreated = await firstInvite.ReadRequiredJsonAsync<CreateInvitationResult>();
        var firstToken = ExtractToken(firstCreated.InviteUrl);

        (await clientInvitations.AcceptAsync(firstToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondInvite = await secondTrainerInvitations.CreateAsync(7);
        var secondCreated = await secondInvite.ReadRequiredJsonAsync<CreateInvitationResult>();
        var secondToken = ExtractToken(secondCreated.InviteUrl);

        var secondAccept = await clientInvitations.AcceptAsync(secondToken);

        secondAccept.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await secondAccept.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("invitation.accept.client_has_another_trainer");
    }

    [Fact]
    public async Task Accept_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var anonymousInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("unauth-trainer"),
            "Str0ngPass!123",
            "Trainer User",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var response = await anonymousInvitations.AcceptAsync(ExtractToken(created.InviteUrl));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Accept_WithInvalidToken_ShouldReturnNotFound()
    {
        var clientHttp = Fixture.CreateClient(handleCookies: false);
        var clientAuth = new AuthTestClient(clientHttp);
        var invitationsClient = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await clientAuth.RegisterAsync(
            UniqueEmail("invalid-token-client"),
            "Str0ngPass!123",
            "Invalid Token Client",
            AuthRoles.Client)).StatusCode.Should().Be(HttpStatusCode.Created);

        await invitationsClient.CopyAuthStateFromAsync(clientAuth);

        var response = await invitationsClient.AcceptAsync("invalid-token");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("invitation.not_found");
    }

    [Fact]
    public async Task Accept_WithTrainerRole_ShouldReturnForbidden()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var secondTrainerHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var secondTrainerAuth = new AuthTestClient(secondTrainerHttp);
        var secondTrainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        (await trainerAuth.RegisterAsync(
            UniqueEmail("source-trainer"),
            "Str0ngPass!123",
            "Source Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await secondTrainerAuth.RegisterAsync(
            UniqueEmail("wrong-role-trainer"),
            "Str0ngPass!123",
            "Wrong Role Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);
        await secondTrainerInvitations.CopyAuthStateFromAsync(secondTrainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var response = await secondTrainerInvitations.AcceptAsync(ExtractToken(created.InviteUrl));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static string ExtractToken(string inviteUrl)
    {
        var uri = new Uri(inviteUrl, UriKind.Absolute);
        return uri.Segments[^1].Trim('/');
    }
}
