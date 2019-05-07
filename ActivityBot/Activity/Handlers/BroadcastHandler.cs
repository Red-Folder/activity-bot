using ActivityBot.Activity.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot.Activity.Handlers
{
    public class BroadcastHandler
    {
        private readonly Configuration _configuration;
        private readonly BroadcastCallbackFactory _factory;

        public BroadcastHandler(Configuration configuration, BroadcastCallbackFactory factory)
        {
            _configuration = configuration;
            _factory = factory;
        }

        public async Task Handle(ITurnContext turnContext, ActivityBotAccessors accessors, CancellationToken cancellationToken)
        {
            if (ShouldHandle(turnContext))
            {
                var message = turnContext.Activity.Text.Substring("broadcast".Length).Trim();
                var payloadType = message.Split(' ')[0].Trim();
                var payload = message.Substring(payloadType.Length).Trim();

                var callback = _factory.GetCallback(payloadType, payload);

                await Process(turnContext, accessors, callback, cancellationToken);
            }
        }

        private bool ShouldHandle(ITurnContext turnContext)
        {
            return turnContext.Activity.Text.StartsWith("broadcast", StringComparison.CurrentCultureIgnoreCase);
        }

        private async Task Process(ITurnContext turnContext, ActivityBotAccessors accessors, ICallback callback, CancellationToken cancellationToken)
        {
            var notificationList = await accessors.NotificationList.GetAsync(turnContext, () => new NotificationList
            {
                Conversations = new List<ConversationReference>()
            });

            foreach (var conversation in notificationList.Conversations)
            {
                try
                {
                    var message = turnContext.Activity.Text.Substring("broadcast".Length);
                    await turnContext.Adapter.ContinueConversationAsync(_configuration.AppId,
                                                                        conversation,
                                                                        callback.Callback(),
                                                                        cancellationToken);
                }
                catch (Exception ex)
                {
                    await turnContext.SendActivityAsync(ex.Message);
                }
            }
        }
    }
}
