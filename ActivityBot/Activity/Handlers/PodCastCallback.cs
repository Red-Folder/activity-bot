using ActivityBot.Activity.Models.Broadcast;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace ActivityBot.Activity.Handlers
{
    public class PodCastCallback : ICallback
    {
        private readonly PodCast _model;

        public PodCastCallback(string json)
        {
            _model = JsonConvert.DeserializeObject<PodCast>(json);
        }

        public BotCallbackHandler Callback()
        {
            return async (turnContext, token) =>
            {
                await turnContext.SendActivityAsync($"New Podcast: {_model.EpisodeName}");
            };
        }
    }
}
