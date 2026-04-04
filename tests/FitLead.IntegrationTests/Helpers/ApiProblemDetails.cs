namespace FitLead.IntegrationTests.Helpers;

public sealed record ApiProblemDetails(
    string? Title,
    string? Detail,
    string? ErrorCode);
