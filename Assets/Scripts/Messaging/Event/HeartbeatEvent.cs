using System;
using System.Linq;

namespace Messaging.Event
{
    public class HeartbeatEvent
    {
        public EventHeader header;
        public HeartbeatPayload payload;

        public static HeartbeatEvent BuildForGame(string gameId)
        {
            var connectedPlayersList = LobbyManager.Instance.players.Select(p => p.username).ToList();
            return new HeartbeatEvent
            {
                header = new EventHeader
                {
                    name = "heartbeat",
                    version = "1",
                    domain = EventDomain.LIVE_FEED,
                    type = EventType.HEARTBEAT,
                    gameId = gameId,
                    timestamp = DateTime.UtcNow
                },
                payload = new HeartbeatPayload
                {
                    message = $"Game: {gameId} sends heartbeat",
                    connectedPlayers = connectedPlayersList
                }
            };
        }
    }
}