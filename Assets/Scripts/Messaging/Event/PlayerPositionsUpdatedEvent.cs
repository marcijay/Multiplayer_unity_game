using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Messaging.Event
{
    public class PlayerPositionDto
    {
        public string username;
        public PlayerLocationCoordinates coordinates;
    }

    public class PlayerLocationCoordinates
    {
        public double x;
        public double y;
        public double z;

        public static PlayerLocationCoordinates fromVector(Vector3 position)
        {
            return new PlayerLocationCoordinates
            {
                x = position.x,
                y = position.y,
                z = position.z
            };
        }
    }

    public class PlayerPositionsUpdatedEvent : IBaseEvent
    {
        public EventHeader header;
        public List<PlayerPositionDto> payload;

        public static PlayerPositionsUpdatedEvent Create(List<PlayerPositionDto> playerPositions)
        {
            return new PlayerPositionsUpdatedEvent
            {
                header = new EventHeader
                {
                    name = "playerPositionsUpdated",
                    version = "1",
                    type = EventType.NONE,
                    domain = EventDomain.STATISTICS,
                    gameId = DataManager.Instance.GameData.GameId,
                    senderUsername = DataManager.Instance.PlayerData.PlayerName,
                    timestamp = DateTime.UtcNow
                },
                payload = playerPositions
            };
        }

        public EventHeader GetHeader()
        {
            return header;
        }
    }
}