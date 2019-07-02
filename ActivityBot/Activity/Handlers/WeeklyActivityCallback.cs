using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Text;

namespace ActivityBot.Activity.Handlers
{
    public class WeeklyActivityCallback : ICallback
    {
        private readonly WeeklyActivity _model;

        public WeeklyActivityCallback(string json)
        {
            _model = JsonConvert.DeserializeObject<WeeklyActivity>(json);
        }

        public BotCallbackHandler Callback()
        {
            return async (turnContext, token) =>
            {
                var builder = new StringBuilder();
                builder.AppendLine("# New Weekly Activity");
                builder.AppendLine($"WeekNo: {_model.WeekNumber}");
                builder.AppendLine($"Url: {_model.ImageUrl}");

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

        private class WeeklyActivity
        {
            public string EventName { get; set; }

            public string InstanceId { get; set; }

            public DateTime Expires { get; set; }

            public bool Expired => Expires < DateTime.Now;

            public string ImageUrl { get; set; }

            public int WeekNumber { get; set; }

            public DateTime From { get; set; }
            public DateTime To { get; set; }
        }
    }
}
