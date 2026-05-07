namespace FitLead.IntegrationTests.Helpers;

public static class ApiCsrfTokenNames
{
    public const string AntiforgeryCookie = "FitLead.Antiforgery";
    public const string RequestTokenCookie = "XSRF-TOKEN";
    public const string RequestHeader = "X-CSRF-TOKEN";
}
