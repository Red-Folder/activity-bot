using ActivityBot.Activity.Models;
using Microsoft.Bot.Builder;
using System;

namespace ActivityBot.Activity
{
    public class ActivityBotAccessors
    {
        public ActivityBotAccessors(ConversationState conversationState, NotificationState notificationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            NotificationState = notificationState ?? throw new ArgumentNullException(nameof(notificationState));
        }

        public static string CounterStateName { get; } = $"{nameof(ActivityBotAccessors)}.CounterState";
        public IStatePropertyAccessor<CounterState> CounterState { get; set; }
        public ConversationState ConversationState { get; }

        public static string NotificationListName { get; } = $"{nameof(ActivityBotAccessors)}.NotificationList";
        public IStatePropertyAccessor<NotificationList> NotificationList { get; set; }
        public NotificationState NotificationState { get; }
    }
}
