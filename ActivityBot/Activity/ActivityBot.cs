using ActivityBot.Activity.Models;
using ActivityBot.Activity.Proxy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ActivityBot(ConversationState conversationState,
                            NotificationState notificationState,
                            ILoggerFactory loggerFactory,
                            ActivityProxy activityProxy,
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
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Text.StartsWith("approve", StringComparison.CurrentCultureIgnoreCase))
                {
                    var request = new ApproveRequest
                    {
                        InstanceId = turnContext.Activity.Text.Split(' ')[1],
                        EventName = turnContext.Activity.Text.Split(' ')[2],
                        Approved = true
                    };

                    try
                    {
                        await _activityProxy.Approve(request);
                        await turnContext.SendActivityAsync("Completed");
                    }
                    catch (Exception ex)
                    {
                        await turnContext.SendActivityAsync($"Failed: {ex.Message}");
                    }

                    return;
                }

                if (turnContext.Activity.Text.StartsWith("decline", StringComparison.CurrentCultureIgnoreCase))
                {
                    var request = new ApproveRequest
                    {
                        InstanceId = turnContext.Activity.Text.Split(' ')[1],
                        EventName = turnContext.Activity.Text.Split(' ')[2],
                        Approved = false
                    };

                    try
                    {
                        await _activityProxy.Approve(request);
                        await turnContext.SendActivityAsync("Completed");
                    }
                    catch (Exception ex)
                    {
                        await turnContext.SendActivityAsync($"Failed: {ex.Message}");
                    }

                    return;
                }

                if (turnContext.Activity.Text.Equals("activity", StringComparison.CurrentCultureIgnoreCase))
                {
                    var awaitingActivities = await _activityProxy.GetAwaiting();

                    if (awaitingActivities != null && awaitingActivities.Count > 0)
                    {
                        foreach (var activity in awaitingActivities)
                        {
                            var reply = turnContext.Activity.CreateReply();

                            var card = new HeroCard(
                                    title: $"Week Nunber: {activity.WeekNumber}",
                                    text: $"Activity for {activity.From.ToShortDateString()} to {activity.To.ToShortDateString()}",
                                    images: new CardImage[] { new CardImage(url: activity.ImageUrl) },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(
                                            title: "Approve",
                                            type: ActionTypes.PostBack,
                                            value: $"approve {activity.InstanceId} {activity.EventName}"),
                                        new CardAction(
                                            title: "Discard",
                                            type: ActionTypes.PostBack,
                                            value: $"discard {activity.InstanceId} {activity.EventName}"),
                                    }
                                );
                            reply.Attachments.Add(card.ToAttachment());

                            await turnContext.SendActivityAsync(reply);
                        }
                        return;
                    }

                    await turnContext.SendActivityAsync("No awaiting activities");
                    return;
                }

                if (turnContext.Activity.Text.Equals("notifications register", StringComparison.CurrentCultureIgnoreCase))
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

                    return;
                }

                if (turnContext.Activity.Text.Equals("broadcast", StringComparison.CurrentCultureIgnoreCase))
                {
                    var notificationList = await _accessors.NotificationList.GetAsync(turnContext, () => new NotificationList
                    {
                        Conversations = new List<ConversationReference>()
                    });

                    foreach (var conversation in notificationList.Conversations)
                    {
                        try
                        {
                            await turnContext.Adapter.ContinueConversationAsync(_configuration.AppId,
                                                                                conversation,
                                                                                CreateCallback("Hello World"),
                                                                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await turnContext.SendActivityAsync(ex.Message);
                        }
                    }

                    return;
                }

                // Get the conversation state from the turn context.
                var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count for this conversation.
                state.TurnCount++;

                // Set the property using the accessor.
                await _accessors.CounterState.SetAsync(turnContext, state);

                // Save the new turn count into the conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext);

                // Echo back to the user whatever they typed.
                var responseMessage = $"Turn {state.TurnCount}: You sent '{turnContext.Activity.Text}'\n";
                await turnContext.SendActivityAsync(responseMessage);

                return;
            }

            await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
        }

        private BotCallbackHandler CreateCallback(string message)
        {
            return async (turnContext, token) =>
            {
                // Send the user a proactive confirmation message.
                await turnContext.SendActivityAsync(message);
            };
        }
    }
}
