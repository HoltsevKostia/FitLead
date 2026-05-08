using System.Net;
using FitLead.Application.Invitations.Commands;
using FitLead.Application.Invitations.Queries;
using FitLead.Domain.Invitations;
using FitLead.Infrastructure.Persistence;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Invitations;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class RevokeInvitationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Revoke_WithOwnerTrainer_ShouldReturnNoContentAndMakePreviewRevoked()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var anonymousHttp = Fixture.CreateClient(handleCookies: false);

        var trainerAuth = new AuthTestClient(trainerHttp);
        var trainerInvitations = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var anonymousInvitations = new InvitationsTestClient(anonymousHttp);

        (await trainerAuth.RegisterAsync(
            UniqueEmail("revoke-trainer"),
            "Str0ngPass!123",
            "Revoke Trainer",
            AuthRoles.Trainer)).StatusCode.Should().Be(HttpStatusCode.Created);

        await trainerInvitations.CopyAuthStateFromAsync(trainerAuth);

        var create = await trainerInvitations.CreateAsync(7);
        var created = await create.ReadRequiredJsonAsync<CreateInvitationResult>();

        var revoke = await trainerInvitations.RevokeAsync(created.InvitationId);

        revoke.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using (var scope = Fixture.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
            var invitation = await db.Invitations.SingleAsync();
            invitation.Status.Should().Be(InvitationStatus.Revoked);
        }

        var preview = await anonymousInvitations.PreviewAsync(ExtractToken(created.InviteUrl));
        preview.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await preview.ReadRequiredJsonAsync<InvitationPreviewDto>();
        payload.Status.Should().Be("Revoked");
        payload.IsJoinable.Should().BeFalse();
    }

    private static string ExtractToken(string inviteUrl)
    {
        var uri = new Uri(inviteUrl, UriKind.Absolute);
        return uri.Segments[^1].Trim('/');
    }
}
