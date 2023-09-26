using System;
using Amazon.SQS;
using Amazon.SQS.Model;
using FishNet.Object;
using Messaging.Dto;
using Messaging.Event;
using Newtonsoft.Json;
using UnityEngine;

namespace Messaging
{
    public sealed class EventSender
    {
        private AwsConfigDto _awsConfig;
        private IAmazonSQS _sqsClient;
        private GameInfoHolderSO _gameData;

        private EventSender()
        {
            _awsConfig = AwsConfigUtils.ProvideAwsConfig();
            _sqsClient = AwsConfigUtils.ProvideSqsClient();
            _gameData = DataManager.Instance.GameData;
        }

        private static EventSender _instance;

        public static EventSender GetInstance()
        {
            return _instance ??= new EventSender();
        }

        public void SendHeartbeat()
        {
            var gameId = _gameData.GameId;
            var heartbeatEvent = HeartbeatEvent.BuildForGame(gameId);
            var sendRequest = new SendMessageRequest
            {
                QueueUrl = _gameData.LiveFeedQueueUrl,
                MessageGroupId = gameId,
                MessageBody = JsonConvert.SerializeObject(heartbeatEvent)
            };
            SendMessage(sendRequest);
        }

        public void SendLiveFeed(IBaseEvent baseEvent)
        {
            var gameId = baseEvent.GetHeader().gameId;
            SendMessageRequest sendRequest = new SendMessageRequest
            {
                QueueUrl = AwsConfigUtils.ResolveQueueNameToUrl(_awsConfig.sqsConfig.liveFeedSqsName),
                MessageGroupId = gameId,
                MessageBody = JsonConvert.SerializeObject(baseEvent)
            };
            SendMessage(sendRequest);
        }
        
        public void SendStatistics(IBaseEvent baseEvent)
        {
            var gameId = baseEvent.GetHeader().gameId;
            SendMessageRequest sendRequest = new SendMessageRequest
            {
                QueueUrl = AwsConfigUtils.ResolveQueueNameToUrl(_awsConfig.sqsConfig.statisticsSqsName),
                MessageGroupId = gameId,
                MessageBody = JsonConvert.SerializeObject(baseEvent)
            };
            SendMessage(sendRequest);
        }
        
        private void SendMessage(SendMessageRequest sendMessageRequest)
        {
            //Debug.Log($"Sending message request: {sendMessageRequest.MessageBody}");
            _sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }
}