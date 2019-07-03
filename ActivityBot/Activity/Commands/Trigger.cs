using ActivityBot.Activity.Models;
using ActivityBot.Activity.Proxy;
using Microsoft.Bot.Builder;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot.Activity.Commands
{
    public class Trigger : ICommand
    {
        private const string KEYWORD = "trigger";

        private const string COMMAND_FORMAT = "trigger {weekNumber} {year}";
        
        private readonly ActivityProxy _activityProxy;

        public string CommandSummary => $"{COMMAND_FORMAT} - Used to manually trigger the weekly activity functions";

        public Trigger(ActivityProxy activityProxy)
        {
            _activityProxy = activityProxy;
        }

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.StartsWith(KEYWORD, StringComparison.CurrentCultureIgnoreCase))
            {
                var parser = new Parser(turnContext.Activity.Text);

                if (!parser.IsValid)
                {
                    await turnContext.SendActivityAsync(parser.ValidationFailureMessage);
                    return true;
                }

                var request = new ManuallyTriggerWeeklyActivityRequest
                {
                    StartFrom = "FromScreenCapture",
                    WeekNumber = parser.WeekNumber,
                    Year = parser.Year
                };

                try
                {
                    await _activityProxy.ManuallyTriggerWeeklyActivity(request);
                    await turnContext.SendActivityAsync("Requested");
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

                if (!int.TryParse(tokens[1], out int weekNumber))
                {
                    IsValid = false;
                    ValidationFailureMessage = $"weekNumber should be a number - {tokens[1]}";
                    return;
                }
                WeekNumber = weekNumber;

                if (!int.TryParse(tokens[2], out int year))
                {
                    IsValid = false;
                    ValidationFailureMessage = $"year should be a number - {tokens[2]}";
                    return;
                }
                Year = year;

                IsValid = true;
            }

            public bool IsValid;
            public string ValidationFailureMessage;

            public int WeekNumber { get; private set; }
            public int Year { get; private set; }
        }
    }
}
