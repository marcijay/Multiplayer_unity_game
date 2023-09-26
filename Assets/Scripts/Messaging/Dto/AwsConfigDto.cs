using System;

namespace Messaging.Dto
{
    public class AwsConfigDto
    {
        public HostConfig hostConfig;
        public string awsEndpoint { get; set; }
        public string awsRegion { get; set; }
        public string awsAccessKey { get; set; }
        public string awsSecretKey { get; set; }
        public string awsAccountId { get; set; }
        public int sqsPollIntervalMilli { get; set; }

        public SqsConfig sqsConfig;
    }

    public class SqsConfig
    {
        public string influenceSqsPrefix;
        public string liveFeedSqsName;
        public string statisticsSqsName;
    }

    public class HostConfig
    {
        public string backendOutsideUrl;
        public bool remoteBackendEnabled;
    }
}