using System;
using System.Threading;
using System.Threading.Tasks;
using ActivityBot.Activity.Models;
using ActivityBot.Activity.Proxy;
using Microsoft.Bot.Builder;

namespace ActivityBot.Activity.Commands
{
    public class ActivityApproval : ICommand
    {
        private const string APPROVAL_KEYWORD = "approve";
        private const string DECLINE_KEYWORD = "decline";

        private const string COMMAND_FORMAT = "approve/ decline {instanceId} {eventName}";

        private readonly ActivityProxy _activityProxy;

        public string CommandSummary => $"{COMMAND_FORMAT} - Used to approve or decline a Weekly Activity";

        public ActivityApproval(ActivityProxy activityProxy)
        {
            _activityProxy = activityProxy;
        }

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.StartsWith(APPROVAL_KEYWORD, StringComparison.CurrentCultureIgnoreCase) ||
                turnContext.Activity.Text.StartsWith(DECLINE_KEYWORD, StringComparison.CurrentCultureIgnoreCase))
            {
                var parser = new Parser(turnContext.Activity.Text);

                if (!parser.IsValid)
                {
                    await turnContext.SendActivityAsync(parser.ValidationFailureMessage);
                    return true;
                }

                var request = new ApproveRequest
                {
                    InstanceId = parser.InstanceId,
                    EventName = parser.EventName,
                    Approved = parser.Approved
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

                return true;
            }

            return false;
        }

        private class Parser
        {
            public Parser(string request)
            {
                var tokens = request.Split(' ');

                if (tokens.Length != 3)
                {
                    IsValid = false;
                    ValidationFailureMessage = $"Expecting format: {COMMAND_FORMAT}";
                    return;
                }

                InstanceId = tokens[1];
                EventName = tokens[2];
                Approved = tokens[0].Equals(APPROVAL_KEYWORD, StringComparison.CurrentCultureIgnoreCase);
                IsValid = true;
            }

            public bool IsValid;
            public string ValidationFailureMessage;

            public string InstanceId { get; private set; }
            public string EventName { get; private set; }
            public bool Approved { get; private set; }
        }
    }
}
