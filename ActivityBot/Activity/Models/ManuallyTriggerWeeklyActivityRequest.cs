using Newtonsoft.Json;

namespace ActivityBot.Activity.Models
{
    public class ManuallyTriggerWeeklyActivityRequest
    {
        [JsonProperty("startFrom")]
        public string StartFrom { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("weekNumber")]
        public int WeekNumber { get; set; }
    }
}
