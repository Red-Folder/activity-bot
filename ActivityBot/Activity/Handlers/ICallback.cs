using Microsoft.Bot.Builder;

namespace ActivityBot.Activity.Handlers
{
    public interface ICallback
    {
        BotCallbackHandler Callback();
    }
}