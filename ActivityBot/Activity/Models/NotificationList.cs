using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace ActivityBot.Activity.Models
{
    public class NotificationList
    {
        public List<ConversationReference> Conversations { get; set; }
    }
}
