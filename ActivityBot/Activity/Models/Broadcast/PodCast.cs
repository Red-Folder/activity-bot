using Newtonsoft.Json;
using System;

namespace ActivityBot.Activity.Models.Broadcast
{
    public class PodCast
    {
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("playing")]
        public bool Playing { get; set; }

        [JsonProperty("feedName")]
        public string FeedName { get; set; }

        [JsonProperty("feedUrl")]
        public string FeedUrl { get; set; }

        [JsonProperty("episodeName")]
        public string EpisodeName { get; set; }

        [JsonProperty("episodeUrl")]
        public string EpisodeUrl { get; set; }

        [JsonProperty("episodeFile")]
        public string EpisodeFile { get; set; }

        [JsonProperty("episodePostUrl")]
        public string EpisodePostUrl { get; set; }

        [JsonProperty("episodeMime")]
        public string EpisodeMime { get; set; }

        [JsonProperty("episodeSummary")]
        public string EpisodeSummary { get; set; }

        [JsonProperty("episodeDuration")]
        public long EpisodeDuration { get; set; }

        [JsonProperty("episodePosition")]
        public long EpisodePosition { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("track")]
        public string Track { get; set; }

        [JsonProperty("actioned")]
        public bool Actioned { get; set; }
    }
}
