namespace FitLead.Api.Hubs
{
    public static class ChatHubGroups
    {
        public static string ForChat(Guid chatId)
        {
            return $"chat:{chatId:D}";
        }
    }
}
