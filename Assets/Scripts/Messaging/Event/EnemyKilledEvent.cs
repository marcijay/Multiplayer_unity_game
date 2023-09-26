using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messaging.Event
{
    public class EnemyKilledEvent : IBaseEvent
    {
        public class EnemyKilledPayload
        {
            public string executingPlayerUsername;

            [JsonConverter(typeof(StringEnumConverter))]
            public GameEnemyType enemyType;
        }

        public EventHeader header;
        public EnemyKilledPayload payload;

        public static EnemyKilledEvent Create(string executingPlayerUsername, GameEnemyType enemyType)
        {
            return new EnemyKilledEvent
            {
                header = new EventHeader
                {
                    name = "enemyKilled",
                    version = "1",
                    domain = EventDomain.STATISTICS,
                    type = EventType.NONE,
                    gameId = DataManager.Instance.GameData.GameId,
                    timestamp = DateTime.UtcNow
                },
                payload = new EnemyKilledPayload
                {
                    executingPlayerUsername = executingPlayerUsername,
                    enemyType = enemyType
                }
            };
        }

        public EventHeader GetHeader()
        {
            return header;
        }
    }
}