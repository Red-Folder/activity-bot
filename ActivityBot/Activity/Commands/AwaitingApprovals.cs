using ActivityBot.Activity.Proxy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot.Activity.Commands
{
    public class AwaitingApprovals : ICommand
    {
        private const string KEYWORD = "awaiting";

        private readonly ActivityProxy _activityProxy;

        public string CommandSummary => $"{KEYWORD} - Gets all awaiting approvals";

        public AwaitingApprovals(ActivityProxy activityProxy)
        {
            _activityProxy = activityProxy;
        }

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.Equals(KEYWORD, StringComparison.CurrentCultureIgnoreCase))
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
                    return true;
                }

                await turnContext.SendActivityAsync("No awaiting activities");
                return true;
            }

            return false;
        }
    }
}
