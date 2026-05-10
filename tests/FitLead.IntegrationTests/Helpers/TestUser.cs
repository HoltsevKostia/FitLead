using FitLead.IntegrationTests.Clients;

namespace FitLead.IntegrationTests.Helpers;

public sealed record TestUser(
    AuthTestClient Auth,
    Guid Id,
    string Email);
