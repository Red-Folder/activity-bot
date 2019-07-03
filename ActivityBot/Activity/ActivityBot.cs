using ActivityBot.Activity.Handlers;
using ActivityBot.Activity.Models;
using ActivityBot.Activity.Proxy;
using ActivityBot.Activity.Commands;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot.Activity
{
    public class ActivityBot : IBot
    {
        private readonly ActivityBotAccessors _accessors;

        private readonly ILogger _logger;
        private readonly ActivityProxy _activityProxy;

        private readonly Configuration _configuration;

        private readonly BroadcastHandler _broadcastHandler;

        private List<ICommand> _commands;

        public ActivityBot(ConversationState conversationState,
                            NotificationState notificationState,
                            ILoggerFactory loggerFactory,
                            ActivityProxy activityProxy,
                            BroadcastHandler broadcastHandler,
                            Configuration configuration)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (notificationState == null)
            {
                throw new System.ArgumentNullException(nameof(notificationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _accessors = new ActivityBotAccessors(conversationState, notificationState)
            {
                CounterState = conversationState.CreateProperty<CounterState>(ActivityBotAccessors.CounterStateName),
                NotificationList = notificationState.CreateProperty<NotificationList>(NotificationStateAccessors.NotificationListName)
            };

            _logger = loggerFactory.CreateLogger<ActivityBot>();
            _logger.LogTrace("Turn start.");

            _activityProxy = activityProxy;

            _configuration = configuration;

            _broadcastHandler = broadcastHandler;

            _commands = new List<ICommand>();
            _commands.Add(new ActivityApproval(_activityProxy));
            _commands.Add(new AwaitingApprovals(_activityProxy));
            _commands.Add(new Notifications(_accessors));
            _commands.Add(new Trigger(_activityProxy));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var done = await Handle(turnContext, cancellationToken);

                if (!done)
                {
                    var builder = new StringBuilder();
                    builder.Append($"Unknown command.  You sent '{turnContext.Activity.Text}'\n\n");
                    builder.Append("Available options:\n\n");
                    _commands.ForEach(x => builder.Append($"{x.CommandSummary}\n\n"));
                    await turnContext.SendActivityAsync(builder.ToString());
                }
                return;
            }

            await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
        }

        private async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var command in _commands)
            {
                if (await command.Handle(turnContext, cancellationToken))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
