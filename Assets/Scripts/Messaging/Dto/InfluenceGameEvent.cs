namespace Messaging.Dto
{
    public class InfluenceGameEvent
    {
        public string eventName { get; set; }
        public string eventVersion { get; set; }
        public string actionType { get; set; }

        public override string ToString()
        {
            return $"Event: {eventName} {eventVersion} {actionType}";
        }
    }
}