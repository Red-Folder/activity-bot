namespace ActivityBot.Activity.Handlers
{
    public class BroadcastCallbackFactory
    {
        public ICallback GetCallback(string payloadType, string payload)
        {
            if (payloadType == "WeekylActivity")
            {
                return new WeeklyActivityCallback(payload);
            }

            return new PodCastCallback(payload);
        }
    }
}
