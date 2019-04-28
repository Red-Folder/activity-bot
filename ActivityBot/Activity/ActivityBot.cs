using ActivityBot.Activity.Models;
using ActivityBot.Activity.Proxy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot.Activity
{
    public class ActivityBot : IBot
    {
        private readonly ActivityBotAccessors _accessors;
        private readonly ILogger _logger;
        private readonly ActivityProxy _activityProxy;

        public ActivityBot(ConversationState conversationState, ILoggerFactory loggerFactory, ActivityProxy activityProxy)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _accessors = new ActivityBotAccessors(conversationState)
            {
                CounterState = conversationState.CreateProperty<CounterState>(ActivityBotAccessors.CounterStateName),
            };

            _logger = loggerFactory.CreateLogger<ActivityBot>();
            _logger.LogTrace("Turn start.");

            _activityProxy = activityProxy;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {

                if (turnContext.Activity.Text == "activity")
                {
                    var awaitingActivities = await _activityProxy.GetAwaiting();

                    if (awaitingActivities != null && awaitingActivities.Count > 0)
                    {
                        foreach (var activity in awaitingActivities)
                        {
                            var awaitingMessage = turnContext.Activity.CreateReply();
                            awaitingMessage.Text = $"Week Nunber: {activity.WeekNumber}";
                            awaitingMessage.TextFormat = "plain";
                            awaitingMessage.Attachments.Add(new Attachment(contentUrl: activity.ImageUrl, name: "Activity Image"));

                            await turnContext.SendActivityAsync(awaitingMessage);
                        }
                        return;
                    }

                    await turnContext.SendActivityAsync("No awaiting activities");
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
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
