using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Producer
{
    public class ExtendedSimpleNotificationServiceClient : AmazonSimpleNotificationServiceClient
    {
        public S3MessageSettings _s3Settings = new S3MessageSettings();
        public IAmazonS3 _s3Client = new AmazonS3Client(RegionEndpoint.EUWest1);//.GetBySystemName(Environment.GetEnvironmentVariable("DEFAULT_AWS_REGION")));

        public ExtendedSimpleNotificationServiceClient(RegionEndpoint region) : base(region)
        {}

        public override async Task<PublishResponse> PublishAsync(PublishRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (BytesHelper.TooLarge(request))
            {
                await _s3Client.UploadMessage(request, _s3Settings);
            }            
            return await base.PublishAsync(request, cancellationToken);      
        }
    }
}

