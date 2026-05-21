namespace FitLead.Api.Hubs
{
    public static class NotificationHubGroups
    {
        public static string ForUser(Guid userId)
        {
            return $"notifications:user:{userId:D}";
        }
    }
}
