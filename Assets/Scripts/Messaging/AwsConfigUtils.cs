using System;
using System.IO;
using System.Linq;
using Amazon.Runtime;
using Amazon.SQS;
using Messaging.Dto;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Messaging
{
    public static class AwsConfigUtils
    {
        // private static readonly string CONFIG_PATH = "./Assets/Config";

        private static AwsConfigDto _awsConfig;

        private static IAmazonSQS _sqsClient;

        static AwsConfigUtils()
        {
            _awsConfig = ReadAwsConfig();
            _sqsClient = InitializeSqsClient();
        }

        private static IAmazonSQS InitializeSqsClient()
        {
            var sqsConfig = new AmazonSQSConfig
            {
                ServiceURL = _awsConfig.awsEndpoint,
                AuthenticationRegion = _awsConfig.awsRegion
            };

            var awsCredentials = new BasicAWSCredentials(_awsConfig.awsAccessKey, _awsConfig.awsSecretKey);

            return new AmazonSQSClient(awsCredentials, sqsConfig);
        }

        private static AwsConfigDto ReadAwsConfig()
        {
            TextAsset configAsset = Addressables.LoadAssetAsync<TextAsset>("Assets/Config/game-local.json")
                .WaitForCompletion();
            // using var reader = new StreamReader(CONFIG_PATH + "/game-local.json");
            var content = configAsset.text;
            var result = JsonConvert.DeserializeObject<AwsConfigDto>(content);
            return result;
        }

        public static AwsConfigDto ProvideAwsConfig()
        {
            return _awsConfig;
        }

        public static IAmazonSQS ProvideSqsClient()
        {
            return _sqsClient;
        }

        public static string ResolveQueueNameToUrl(string queueName)
        {
            return _awsConfig.awsEndpoint + "/" + _awsConfig.awsAccountId + "/" + queueName;
        }

        public static string ResolveQueueUrlToName(string queueUrl)
        {
            return queueUrl.Split("/").Last();
        }
    }
}