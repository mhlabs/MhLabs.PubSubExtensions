using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Producer
{
    public class ExtendedSimpleNotificationServiceClient : AmazonSimpleNotificationServiceClient
    {
        public IMessageDeliverySettings _messageDeliverySettings = new MessageDeliverySettings();
        public IAmazonS3 _s3Client = new AmazonS3Client();
        public IAmazonStepFunctionsClientAdapter _stepFunctions = new AmazonStepFunctionsClientAdapter(new AmazonStepFunctionsClient());

        public ExtendedSimpleNotificationServiceClient() : base() { }
        public ExtendedSimpleNotificationServiceClient(RegionEndpoint region) : base(region) { }
        public ExtendedSimpleNotificationServiceClient(AmazonSimpleNotificationServiceConfig config) : base(config) { }
        public ExtendedSimpleNotificationServiceClient(AWSCredentials credentials) : base(credentials) { }
        public ExtendedSimpleNotificationServiceClient(AWSCredentials credentials, RegionEndpoint region) : base(credentials, region) { }
        public ExtendedSimpleNotificationServiceClient(AWSCredentials credentials, AmazonSimpleNotificationServiceConfig clientConfig) : base(credentials, clientConfig) { }
        public ExtendedSimpleNotificationServiceClient(string awsAccessKeyId, string awsSecretAccessKey) : base(awsAccessKeyId, awsSecretAccessKey) { }
        public ExtendedSimpleNotificationServiceClient(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region) : base(awsAccessKeyId, awsSecretAccessKey, region) { }
        public ExtendedSimpleNotificationServiceClient(string awsAccessKeyId, string awsSecretAccessKey, string awsSessionToken) : base(awsAccessKeyId, awsSecretAccessKey, awsSessionToken) { }
        public ExtendedSimpleNotificationServiceClient(string awsAccessKeyId, string awsSecretAccessKey, AmazonSimpleNotificationServiceConfig clientConfig) : base(awsAccessKeyId, awsSecretAccessKey, clientConfig) { }
        public ExtendedSimpleNotificationServiceClient(string awsAccessKeyId, string awsSecretAccessKey, string awsSessionToken, AmazonSimpleNotificationServiceConfig clientConfig) : base(awsAccessKeyId, awsSecretAccessKey, awsSessionToken, clientConfig) { }
        public ExtendedSimpleNotificationServiceClient(string awsAccessKeyId, string awsSecretAccessKey, string awsSessionToken, RegionEndpoint region) : base(awsAccessKeyId, awsSecretAccessKey, awsSessionToken, region) { }


        public override async Task<PublishResponse> PublishAsync(PublishRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(request.Subject)) {
                request.Subject = "#"; // To allow for automatic http forwarding
            }
            if (request.MessageAttributes.ContainsKey(Constants.DelaySeconds) && int.Parse(request.MessageAttributes[Constants.DelaySeconds].StringValue) > 0)
            {
                if (BytesHelper.TooLarge(request, 32000))
                {
                    await _s3Client.PubSubS3Query(request, _messageDeliverySettings, cancellationToken);
                }

                return await _stepFunctions.PublishAsync(request, _messageDeliverySettings.StateMachine, cancellationToken);
            }

            if (BytesHelper.TooLarge(request))
            {
                await _s3Client.PubSubS3Query(request, _messageDeliverySettings, cancellationToken);
            }

            return await base.PublishAsync(request, cancellationToken);
        }
    }
}

