using System.Net;
using FitLead.Application.Invitations.Commands;
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
public sealed class CreateInvitationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_WithValidSevenDayPayload_ShouldReturnCreatedAndPersistTokenHashOnly()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var authClient = new AuthTestClient(trainerHttp);
        var invitationsClient = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail("invitation-trainer");

        var register = await authClient.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Invitation Trainer",
            AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        await invitationsClient.CopyAuthStateFromAsync(authClient);

        var before = DateTime.UtcNow;
        var response = await invitationsClient.CreateAsync(7);
        var after = DateTime.UtcNow;

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.ReadRequiredJsonAsync<CreateInvitationResult>();
        payload.InvitationId.Should().NotBeEmpty();
        payload.InviteUrl.Should().StartWith("http://localhost:3000/invite/");

        var token = ExtractToken(payload.InviteUrl);
        token.Should().NotBeNullOrWhiteSpace();

        var expectedMin = before.AddDays(7).AddMinutes(-1);
        var expectedMax = after.AddDays(7).AddMinutes(1);
        payload.ExpiresAtUtc.Should().BeOnOrAfter(expectedMin);
        payload.ExpiresAtUtc.Should().BeOnOrBefore(expectedMax);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var invitation = await db.Invitations.SingleAsync();
        invitation.Id.Should().Be(payload.InvitationId);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.TokenHash.Should().NotBeNullOrWhiteSpace();
        invitation.TokenHash.Should().NotBe(token);
        invitation.AcceptedByClientId.Should().BeNull();
        invitation.AcceptedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Create_WithInvalidExpiryDays_ShouldReturnBadRequestWithValidationProblem()
    {
        var trainerHttp = Fixture.CreateClient(handleCookies: false);
        var authClient = new AuthTestClient(trainerHttp);
        var invitationsClient = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        var register = await authClient.RegisterAsync(
            UniqueEmail("invalid-expiry"),
            "Str0ngPass!123",
            "Trainer User",
            AuthRoles.Trainer);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        await invitationsClient.CopyAuthStateFromAsync(authClient);

        var response = await invitationsClient.CreateAsync(10);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("invitation.create.expires_in_days_invalid");
        problem.Title.Should().Be("Validation");
    }

    [Fact]
    public async Task Create_WithClientRole_ShouldReturnForbidden()
    {
        var clientHttp = Fixture.CreateClient(handleCookies: false);
        var authClient = new AuthTestClient(clientHttp);
        var invitationsClient = new InvitationsTestClient(Fixture.CreateClient(handleCookies: false));

        var register = await authClient.RegisterAsync(
            UniqueEmail("client-create"),
            "Str0ngPass!123",
            "Client User",
            AuthRoles.Client);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        await invitationsClient.CopyAuthStateFromAsync(authClient);

        var response = await invitationsClient.CreateAsync(7);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static string ExtractToken(string inviteUrl)
    {
        var uri = new Uri(inviteUrl, UriKind.Absolute);
        return uri.Segments[^1].Trim('/');
    }
}
