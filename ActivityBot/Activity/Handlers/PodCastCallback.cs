using ActivityBot.Activity.Models.Broadcast;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Text;

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
                var builder = new StringBuilder();
                builder.AppendLine("# New podcast");
                builder.AppendLine($"Feed: {_model.FeedName}");
                builder.AppendLine($"Episode: {_model.EpisodeName}");
                builder.AppendLine($"Category: {_model.Category}");
                builder.AppendLine($"Url: {_model.EpisodeUrl}");

                var activity = new Microsoft.Bot.Schema.Activity
                {
                    Type = ActivityTypes.Message,
                    From = new ChannelAccount
                    {
                        Id = "ActivityBot",
                        Name = "Activity Bot"
                    },
                    TextFormat = "markdown",
                    Text = builder.ToString()
                };

                await turnContext.SendActivityAsync(activity);
            };
        }
    }
}
