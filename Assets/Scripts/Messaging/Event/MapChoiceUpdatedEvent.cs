using System;

namespace Messaging.Event
{
    public class MapChoiceUpdatedEvent : IBaseEvent
    {
        public class MapChoiceUpdatedPayload
        {
            public string mapName;
        }

        public EventHeader header;
        public MapChoiceUpdatedPayload payload;

        public static MapChoiceUpdatedEvent Create(string mapName)
        {
            return new MapChoiceUpdatedEvent
            {
                header = new EventHeader("mapChoiceUpdated", "1", EventDomain.LIVE_FEED, EventType.GAME_STATUS_CHANGED),
                payload = new MapChoiceUpdatedPayload
                {
                    mapName = mapName
                }
            };
        }

        public EventHeader GetHeader()
        {
            return header;
        }
    }
}