using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot.Activity.Commands
{
    public interface ICommand
    {
        Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken);
        string CommandSummary { get; }
    }
}
