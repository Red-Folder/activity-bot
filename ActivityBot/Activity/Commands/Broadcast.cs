using System;
using System.Threading;
using System.Threading.Tasks;
using ActivityBot.Activity.Handlers;
using Microsoft.Bot.Builder;

namespace ActivityBot.Activity.Commands
{
    public class Broadcast : ICommand
    {
        private const string KEYWORD = "broadcast";

        private readonly BroadcastHandler _broadcastHandler;
        private readonly ActivityBotAccessors _accessors;

        public Broadcast(BroadcastHandler broadcastHandler, ActivityBotAccessors accessors)
        {
            _broadcastHandler = broadcastHandler;
            _accessors = accessors;
        }

        public string CommandSummary => $"{KEYWORD} - Used to broadcast";

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.StartsWith("broadcast", StringComparison.CurrentCultureIgnoreCase))
            {
                await _broadcastHandler.Handle(turnContext, _accessors, cancellationToken);

                return true;
            }

            return false;
        }
    }
}
