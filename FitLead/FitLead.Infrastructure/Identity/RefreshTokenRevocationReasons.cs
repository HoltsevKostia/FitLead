namespace FitLead.Infrastructure.Identity
{
    public static class RefreshTokenRevocationReasons
    {
        public const string Rotated = "Rotated";
        public const string ReuseDetected = "ReuseDetected";
        public const string Logout = "Logout";
        public const string Expired = "Expired";
    }
}
