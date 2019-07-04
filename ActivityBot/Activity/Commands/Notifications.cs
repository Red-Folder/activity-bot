using ActivityBot.Activity.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot.Activity.Commands
{
    public class Notifications: ICommand
    {
        private const string TRIGGER_KEYWORD = "notifications";
        private const string REGISTER_KEYWORD = "register";
        private const string DEREGISTER_KEYWORD = "deregister";

        private const string COMMAND_FORMAT = "notifications register/ deregister";

        private readonly ActivityBotAccessors _accessors;

        public string CommandSummary => $"{COMMAND_FORMAT} - Used to register/ deregister for notification broadcasts";

        public Notifications(ActivityBotAccessors accessors)
        {
            _accessors = accessors;
        }

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.StartsWith(TRIGGER_KEYWORD, StringComparison.CurrentCultureIgnoreCase))
            {
                var parser = new Parser(turnContext.Activity.Text);

                if (!parser.IsValid)
                {
                    await turnContext.SendActivityAsync(parser.ValidationFailureMessage);
                    return true;
                }

                if (parser.Register)
                {
                    await Register(turnContext, cancellationToken);
                }
                else
                {
                    await Deregister(turnContext, cancellationToken);
                }

                return true;
            }

            return false;
        }

        private async Task Register(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var notificationList = await _accessors.NotificationList.GetAsync(turnContext, () => new NotificationList
            {
                Conversations = new List<ConversationReference>()
            });

            if (notificationList.Conversations.Any(x => x.Conversation.Id == turnContext.Activity.GetConversationReference().Conversation.Id))
            {
                await turnContext.SendActivityAsync("This conversation is already registered");
            }
            else
            {
                notificationList.Conversations.Add(turnContext.Activity.GetConversationReference());
                await _accessors.NotificationList.SetAsync(turnContext, notificationList);
                await _accessors.NotificationState.SaveChangesAsync(turnContext);

                await turnContext.SendActivityAsync("Conversation now registered");
            }
        }

        private async Task Deregister(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var notificationList = await _accessors.NotificationList.GetAsync(turnContext, () => new NotificationList
            {
                Conversations = new List<ConversationReference>()
            });

            if (notificationList.Conversations.Any(x => x.Conversation.Id == turnContext.Activity.GetConversationReference().Conversation.Id))
            {
                notificationList.Conversations.RemoveAll(x => x.Conversation.Id == turnContext.Activity.GetConversationReference().Conversation.Id);
                await _accessors.NotificationList.SetAsync(turnContext, notificationList);
                await _accessors.NotificationState.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync("Conversation now deregistered");
            }
            else
            {
                await turnContext.SendActivityAsync("This conversation is not registered");
            }
        }

        private class Parser
        {
            public Parser(string request)
            {
                var tokens = request.Split(' ');

                if (tokens.Length == 2)
                {
                    if (tokens[1].Equals(REGISTER_KEYWORD, StringComparison.CurrentCultureIgnoreCase) ||
                        tokens[1].Equals(DEREGISTER_KEYWORD, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Register = tokens[1].Equals(REGISTER_KEYWORD, StringComparison.CurrentCultureIgnoreCase);
                        IsValid = true;
                        return;
                    }
                }

                IsValid = false;
                ValidationFailureMessage = $"Expecting format: {COMMAND_FORMAT}";
            }

            public bool IsValid;
            public string ValidationFailureMessage;

            public bool Register { get; private set; }
        }
    }
}
