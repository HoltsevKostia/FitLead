namespace FitLead.Api.Auth
{
    internal static class CsrfTokenNames
    {
        public const string AntiforgeryCookie = "FitLead.Antiforgery";
        public const string RequestTokenCookie = "XSRF-TOKEN";
        public const string RequestHeader = "X-CSRF-TOKEN";
    }
}
