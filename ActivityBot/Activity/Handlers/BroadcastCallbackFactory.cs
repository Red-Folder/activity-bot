namespace ActivityBot.Activity.Handlers
{
    public class BroadcastCallbackFactory
    {
        public PodCastCallback GetCallback(string payloadType, string payload)
        {
            return new PodCastCallback(payload);
        }
    }
}
