using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messaging.Event
{
    public class GameStatusChangedEvent : IBaseEvent
    {
        public class GameStatusChangedPayload
        {
            public string message;

            [JsonConverter(typeof(StringEnumConverter))]
            public GameStatus currentGameStatus;
        }

        public EventHeader header;
        public GameStatusChangedPayload payload;

        public static GameStatusChangedEvent Create(GameStatus targetStatus, string message)
        {
            return new GameStatusChangedEvent
            {
                header = new EventHeader
                {
                    name = "gameStatusChangedEvent",
                    version = "1",
                    type = EventType.GAME_STATUS_CHANGED,
                    domain = EventDomain.LIVE_FEED,
                    gameId = DataManager.Instance.GameData.GameId,
                    senderUsername = DataManager.Instance.PlayerData.PlayerName,
                    timestamp = DateTime.UtcNow
                },
                payload = new GameStatusChangedPayload
                {
                    message = message,
                    currentGameStatus = targetStatus
                }
            };
        }

        public EventHeader GetHeader()
        {
            return header;
        }
    }
}