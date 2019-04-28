using Newtonsoft.Json;
using System;

namespace ActivityBot.Activity.Models
{
    public class Awaiting
    {
        [JsonProperty("eventName")]
        public string EventName { get; set; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("weekNumber")]
        public int WeekNumber { get; set; }

        [JsonProperty("from")]
        public DateTime From { get; set; }

        [JsonProperty("to")]
        public DateTime To { get; set; }
    }
}
