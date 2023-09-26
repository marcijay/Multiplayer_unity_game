using System;

namespace Messaging.Event
{
    public class PlayerDiedEvent : IBaseEvent
    {
        public class PlayerDiedPayload
        {
            public string username;
        }

        public EventHeader header;
        public PlayerDiedPayload payload;

        public static PlayerDiedEvent Create(string diedPlayerUsername)
        {
            return new PlayerDiedEvent
            {
                header = new EventHeader
                {
                    name = "playerDied",
                    version = "1",
                    type = EventType.NONE,
                    domain = EventDomain.LIVE_FEED,
                    gameId = DataManager.Instance.GameData.GameId,
                    senderUsername = DataManager.Instance.PlayerData.PlayerName,
                    timestamp = DateTime.UtcNow
                },
                payload = new PlayerDiedPayload
                {
                    username = diedPlayerUsername
                }
            };
        }

        public EventHeader GetHeader()
        {
            return header;
        }
    }
}