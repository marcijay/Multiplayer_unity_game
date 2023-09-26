using System;

namespace Messaging.Event
{
    public class PlayerConnectionActivityEvent : IBaseEvent
    {
        public EventHeader header { get; set; }
        public Payload payload { get; set; }

        public class Payload
        {
            public string username { get; set; }
            public string connectionActivityType { get; set; }
            public string message { get; set; }
        }

        public static PlayerConnectionActivityEvent buildFor(string username, string activityType)
        {
            var msg = $"Player {username} has {activityType.ToLower()} the game";
            return new PlayerConnectionActivityEvent
            {
                header = new EventHeader
                {
                    name = "playerConnectionActivity",
                    version = "1",
                    domain = EventDomain.LIVE_FEED,
                    gameId = DataManager.Instance.GameData.GameId,
                    type = EventType.PLAYER_CONNECTION_ACTIVITY,
                    timestamp = DateTime.UtcNow
                },
                payload = new Payload
                {
                    username = username,
                    connectionActivityType = activityType,
                    message = msg
                }
            };
        }

        public EventHeader GetHeader()
        {
            return header;
        }
    }
}