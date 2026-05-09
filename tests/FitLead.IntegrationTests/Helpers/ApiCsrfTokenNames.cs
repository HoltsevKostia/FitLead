namespace FitLead.IntegrationTests.Helpers;

public static class ApiCsrfTokenNames
{
    public const string AntiforgeryCookie = "FitLead.Antiforgery";
    public const string RequestTokenCookie = "FitLead.XSRF-TOKEN";
    public const string RequestHeader = "X-CSRF-TOKEN";
}
