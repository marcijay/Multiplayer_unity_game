using Amazon.SQS;
using System;
using System.Threading;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Messaging
{
    public class SqsListener<T> : IDisposable
    {
        private readonly string _queueUrl;
        private readonly IAmazonSQS _sqsClient;
        private readonly Action<T> _consumerCallback;
        private readonly int _pollIntervalMillis;
#nullable enable
        private Timer? _timer;
#nullable disable

        public SqsListener(string queueUrl, IAmazonSQS sqsClient, Action<T> consumerCallback,
            int pollIntervalMillis = 1000)
        {
            _queueUrl = queueUrl;
            _sqsClient = sqsClient;
            _consumerCallback = consumerCallback;
            _pollIntervalMillis = pollIntervalMillis;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public void Start()
        {
            _timer = new Timer(Poll, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_pollIntervalMillis));
        }

        private async void Poll(object state)
        {
            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 0
            };

            var response = await _sqsClient.ReceiveMessageAsync(receiveRequest);

            response.Messages.ForEach(m =>
            {
                if (ProcessMessage(m))
                {
                    DeleteMessage(_sqsClient, m, _queueUrl);
                }
                else
                {
                    TrySendToDlq();
                }
            });
        }

        private bool ProcessMessage(Message message)
        {
            try
            {
                _consumerCallback(ConvertPayload(message));
                return true;
            }
            catch (JsonException e)
            {
                Debug.LogError($"Failed to deserialize message: {message.Body} due to: {e}.");
                return false;
            }
        }

        private T ConvertPayload(Message message)
        {
            return JsonConvert.DeserializeObject<T>(message.Body);
        }

        private static async void DeleteMessage(
            IAmazonSQS sqsClient, Message message, string qUrl)
        {
            Debug.Log($"\nDeleting message {message.MessageId} from queue...");
            await sqsClient.DeleteMessageAsync(qUrl, message.ReceiptHandle);
        }

        private void TrySendToDlq()
        {
            throw new NotImplementedException();
        }

        public int GetPollInterval()
        {
            return _pollIntervalMillis;
        }
    }
}