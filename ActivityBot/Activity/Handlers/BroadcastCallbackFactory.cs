namespace ActivityBot.Activity.Handlers
{
    public class BroadcastCallbackFactory
    {
        public PodCastCallback GetCallback(string payloadType, string payload)
        {
            if (payloadType == "WeekylActivity")
            {
                return new WeekylActivityCallback(payload);
            }

            return new PodCastCallback(payload);
        }
    }
}
