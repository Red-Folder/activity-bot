using Newtonsoft.Json;

namespace ActivityBot.Activity.Models
{
    public class ApproveRequest
    {
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }
        [JsonProperty("eventName")]
        public string EventName { get; set; }
        [JsonProperty("approved")]
        public bool Approved { get; set; }
    }
}
