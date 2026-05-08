using System.Net;
using FitLead.Application.Invitations.Commands;
using FitLead.Application.Invitations.Queries;
using FitLead.Infrastructure.Persistence;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Invitations;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class InvitationPreviewTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Preview_WithValidPendingInvite_ShouldBePublicAndJoinable()
    {
        var trainerClient = Fixture.CreateClient(handleCookies: false);
        var authClient = new AuthTestClient(trainerClient);
        var invitationsClient = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var anonymousClient = Fixture.CreateClient(handleCookies: false);
        var anonymousInvitationsClient = new InvitationsTestClient(anonymousClient);

        var register = await authClient.RegisterAsync(
            UniqueEmail("preview-trainer"),
            "Str0ngPass!123",
            "Preview Trainer",
            AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        await invitationsClient.CopyAuthStateFromAsync(authClient);

        var create = await invitationsClient.CreateAsync(7);
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var preview = await anonymousInvitationsClient.PreviewAsync(ExtractToken(created.InviteUrl));

        preview.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await preview.ReadRequiredJsonAsync<InvitationPreviewDto>();
        payload.Status.Should().Be("Pending");
        payload.IsJoinable.Should().BeTrue();
        payload.Trainer.FullName.Should().Be("Preview Trainer");
    }

    [Fact]
    public async Task Preview_WithInvalidToken_ShouldReturnNotFound()
    {
        var invitationsClient = new InvitationsTestClient(HttpClient);

        var response = await invitationsClient.PreviewAsync("invalid-token");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("invitation.not_found");
    }

    [Fact]
    public async Task Preview_WithAcceptedInvite_ShouldReturnAcceptedAndNotJoinable()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var clientHttp = Fixture.CreateClient(handleCookies: false);
        var anonymousHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var clientAuth = new AuthTestClient(clientHttp);
        var clientInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var anonymousInvitations = new InvitationsTestClient(anonymousHttp);

        var trainerEmail = UniqueEmail("accepted-preview-trainer");
        var clientEmail = UniqueEmail("accepted-preview-client");

        (await trainerAuth.RegisterAsync(trainerEmail, "Str0ngPass!123", "Accepted Trainer", AuthRoles.Trainer))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await clientAuth.RegisterAsync(clientEmail, "Str0ngPass!123", "Accepted Client", AuthRoles.Client))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);
        await clientInvitations.CopyAuthStateFromAsync(clientAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();
        var token = ExtractToken(created.InviteUrl);

        var accept = await clientInvitations.AcceptAsync(token);
        accept.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var preview = await anonymousInvitations.PreviewAsync(token);

        preview.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await preview.ReadRequiredJsonAsync<InvitationPreviewDto>();
        payload.Status.Should().Be("Accepted");
        payload.IsJoinable.Should().BeFalse();
    }

    [Fact]
    public async Task Preview_WithRevokedInvite_ShouldReturnRevokedAndNotJoinable()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var anonymousHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var anonymousInvitations = new InvitationsTestClient(anonymousHttp);

        (await trainerAuth.RegisterAsync(
            UniqueEmail("revoked-preview-trainer"),
            "Str0ngPass!123",
            "Revoked Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();
        var token = ExtractToken(created.InviteUrl);

        var revoke = await trainerInvitations.RevokeAsync(created.InvitationId);
        revoke.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var preview = await anonymousInvitations.PreviewAsync(token);

        preview.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await preview.ReadRequiredJsonAsync<InvitationPreviewDto>();
        payload.Status.Should().Be("Revoked");
        payload.IsJoinable.Should().BeFalse();
    }

    [Fact]
    public async Task Preview_WithExpiredInvite_ShouldReturnExpiredAndNotJoinable()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var anonymousHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var anonymousInvitations = new InvitationsTestClient(anonymousHttp);

        (await trainerAuth.RegisterAsync(
            UniqueEmail("expired-preview-trainer"),
            "Str0ngPass!123",
            "Expired Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();
        var token = ExtractToken(created.InviteUrl);

        await using (var scope = Fixture.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
            var invitation = await db.Invitations.SingleAsync();
            db.Entry(invitation)
                .Property(x => x.ExpiresAtUtc)
                .CurrentValue = DateTime.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();
        }

        var preview = await anonymousInvitations.PreviewAsync(token);

        preview.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await preview.ReadRequiredJsonAsync<InvitationPreviewDto>();
        payload.Status.Should().Be("Expired");
        payload.IsJoinable.Should().BeFalse();
    }

    private static string ExtractToken(string inviteUrl)
    {
        var uri = new Uri(inviteUrl, UriKind.Absolute);
        return uri.Segments[^1].Trim('/');
    }
}
