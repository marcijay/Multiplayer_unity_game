using System;
using System.Collections;
using System.Linq;
using Messaging.Event;
using UnityEngine;
using UnityEngine.Serialization;

namespace Messaging
{
    public class HeartbeatGenerator : MonoBehaviour
    {
        [SerializeField] public int intervalSeconds = 15;
        [SerializeField] public float playerPositionUpdateIntervalSeconds = 5;

        private bool _isAlive;
        public static bool IsGameRunning;

        private void Start()
        {
            _isAlive = true;
            IsGameRunning = false;

            StartCoroutine(HeartbeatEmitter());
            StartCoroutine(PlayerPositionEmitter());
        }

        private void OnApplicationQuit()
        {
            _isAlive = false;
            IsGameRunning = false;
        }

        IEnumerator HeartbeatEmitter()
        {
            while (_isAlive)
            {
                yield return new WaitForSeconds(intervalSeconds);
                EventSender.GetInstance().SendHeartbeat();
            }
        }

        IEnumerator PlayerPositionEmitter()
        {
            while (true)
            {
                if (IsGameRunning)
                {
                    SendPlayerPositionsUpdate();
                }

                yield return new WaitForSeconds(playerPositionUpdateIntervalSeconds);
            }
        }

        private void SendPlayerPositionsUpdate()
        {
            var playerPositions = LobbyManager.Instance.players
                .Where(p => p.controlledPawn != null)
                .Select(p => new PlayerPositionDto
                {
                    username = p.username,
                    coordinates = PlayerLocationCoordinates.fromVector(p.controlledPawn.transform.position)
                }).ToList();

            // TODO no alive players
            var evnt = PlayerPositionsUpdatedEvent.Create(playerPositions);
            EventSender.GetInstance().SendLiveFeed(evnt);
        }
    }
}