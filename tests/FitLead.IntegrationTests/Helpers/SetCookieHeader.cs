namespace FitLead.IntegrationTests.Helpers;

public sealed record SetCookieHeader(
    string Name,
    string Value,
    bool HttpOnly,
    string? Path);
