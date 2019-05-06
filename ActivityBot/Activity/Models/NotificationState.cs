using Microsoft.Bot.Builder;

namespace ActivityBot.Activity.Models
{
    public class NotificationState : BotState
    {
        private const string StorageKey = "ActivityBot.NotificationState";

        public NotificationState(IStorage storage)
        : base(storage, StorageKey)
        {
        }

        protected override string GetStorageKey(ITurnContext turnContext) => StorageKey;
    }
}
