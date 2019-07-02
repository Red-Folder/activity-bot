using System.Collections.Generic;
using System.Threading.Tasks;
using ActivityBot.Activity.Models;

namespace ActivityBot.Activity.Proxy
{
    public interface IActivityProxy
    {
        Task<List<Awaiting>> GetAwaiting();
        Task Approve(ApproveRequest request);
        Task ManuallyTriggerWeeklyActivity(ManuallyTriggerWeeklyActivityRequest request);
    }
}