using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Producer
{
    public class ExtendedSQSClient : AmazonSQSClient
    {
        public MessageDeliverySettings _s3Settings = new MessageDeliverySettings();
        public IAmazonS3 _s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("DEFAULT_AWS_REGION")));

        public override async Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (BytesHelper.TooLarge(request))
            {
                await _s3Client.PubSubS3Query(request, _s3Settings);
            }
            return await base.SendMessageAsync(request, cancellationToken);
        }
    }
}

