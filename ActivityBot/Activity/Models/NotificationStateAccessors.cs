using Microsoft.Bot.Builder;
using System;

namespace ActivityBot.Activity.Models
{
    public class NotificationStateAccessors
    {
        public NotificationStateAccessors(NotificationState notificationState)
        {
            NotificationState = notificationState ?? throw new ArgumentNullException(nameof(notificationState));
        }

        public static string NotificationListName { get; } = $"{nameof(ActivityBotAccessors)}.NotificationList";

        public IStatePropertyAccessor<NotificationState> NotificationList { get; set; }

        public NotificationState NotificationState { get; }
    }
}
