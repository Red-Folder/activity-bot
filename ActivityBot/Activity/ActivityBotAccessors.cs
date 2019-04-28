using ActivityBot.Activity.Models;
using Microsoft.Bot.Builder;
using System;

namespace ActivityBot.Activity
{
    public class ActivityBotAccessors
    {
        public ActivityBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string CounterStateName { get; } = $"{nameof(ActivityBotAccessors)}.CounterState";

        public IStatePropertyAccessor<CounterState> CounterState { get; set; }

        public ConversationState ConversationState { get; }
    }
}
