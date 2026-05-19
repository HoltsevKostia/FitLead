namespace FitLead.Application.Common.Outbox
{
    public static class OutboxEventTypes
    {
        public static class Messenger
        {
            public const string ChatMessageCreated = "Messenger.ChatMessageCreated";
            public const string VideoReportSubmitted = "Messenger.VideoReportSubmitted";
            public const string VideoReportReviewed = "Messenger.VideoReportReviewed";
        }

        public static class Training
        {
            public const string ProgramAssigned = "Training.ProgramAssigned";
        }
    }
}
