namespace ActivityBot.Activity.Models
{
    public class ActivityProxyConfiguration
    {
        public string GetPendingApprovalsUrl { get; set; }
        public string ApproveUrl { get; set; }
        public string ManuallyTriggerWeeklyActivityUrl { get; set; }

        public string StateStorageConnectionString { get; set; }
        public string StateStorageContainer { get; set; }
    }
}
