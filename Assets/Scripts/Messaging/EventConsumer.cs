using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS;
using FishNet.Object;
using Messaging.Dto;
using Messaging.Event;
using Unity.VisualScripting;
using UnityEngine;

namespace Messaging
{
    public class EventConsumer : MonoBehaviour
    {
        private AwsConfigDto _awsConfig;
        private IAmazonSQS _sqsClient;
        private List<IDisposable> _listeners = new List<IDisposable>();

        private void Awake()
        {
            _awsConfig = AwsConfigUtils.ProvideAwsConfig();
            _sqsClient = AwsConfigUtils.ProvideSqsClient();
        }

        void Start()
        {
            // DEV:
            RegisterSqsListener<HeartbeatPayload>("game-test", ConsumeTestEvents);
        }

        private void OnApplicationQuit()
        {
            _listeners.ForEach(listener => listener.Dispose());
        }

        public void RegisterSqsListener<T>(string queueName, Action<T> consumerCallback)
        {
            try
            {
                var sqsListener = new SqsListener<T>(
                    AwsConfigUtils.ResolveQueueNameToUrl(queueName),
                    _sqsClient,
                    consumerCallback,
                    pollIntervalMillis: _awsConfig.sqsPollIntervalMilli);
                sqsListener.Start();
                _listeners.Add(sqsListener);
                Debug.Log("Registered SqsListener for queue: " + queueName);
            }
            catch (Exception e)
            {
                Debug.LogError("Registration failed for: " + queueName);
                Debug.LogError(e.StackTrace);
            }
        }

        public void RegisterInfluenceSqsListener(string gameId)
        {
            RegisterSqsListener<InfluenceEvent>(
                $"{_awsConfig.sqsConfig.influenceSqsPrefix}-{gameId}.fifo",
                ConsumeInfluenceEvents);
        }

        // Callback functions
        private void ConsumeTestEvents(HeartbeatPayload heartbeatPayload)
        {
            Debug.Log($"[ConsumeTestEvents] Received event: {heartbeatPayload.message}");
        }

        public void ConsumeInfluenceEvents(InfluenceEvent influenceEvent)
        {
            Debug.Log(
                $"[InfluenceConsumer] - Received event: ({influenceEvent.header.name}, {influenceEvent.header.version}) - payload: {influenceEvent.payload}");

            var serverPlayer = Player.LocalInstance;

            Debug.Log("Action to Player queue sent");
            serverPlayer.ActionsQueue.Enqueue(influenceEvent);
        }
    }
}