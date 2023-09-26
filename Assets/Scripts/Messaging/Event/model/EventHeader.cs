using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messaging
{
    public class EventHeader
    {
        public string name { get; set; }
        public string version { get; set; }
        public string gameId { get; set; }
        public string senderUsername { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventDomain domain { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventType type { get; set; }

        public DateTime timestamp;

        public EventHeader()
        {
        }

        public EventHeader(string name, string version, EventDomain domain, EventType type)
        {
            this.name = name;
            this.version = version;
            gameId = DataManager.Instance.GameData.GameId;
            senderUsername = DataManager.Instance.PlayerData.PlayerName;
            this.domain = domain;
            this.type = type;
            timestamp = DateTime.UtcNow;
        }
    }
}