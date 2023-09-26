using System.Collections.Generic;

namespace Messaging.Event
{
    public class HeartbeatPayload
    {
        public string message;
        public List<string> connectedPlayers;
    }
}