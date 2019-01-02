using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions
{
    internal static class S3Extension
    {


        internal static async Task PubSubS3Query(this IAmazonS3 s3Client, PublishRequest request, IMessageDeliverySettings deliverySettings = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = await UploadToS3(s3Client, request, deliverySettings, cancellationToken);            
            request.Message = "#";
//            request.MessageAttributes = new Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue>();
            request.MessageAttributes.Add(Constants.PubSubBucket, new Amazon.SimpleNotificationService.Model.MessageAttributeValue { StringValue = deliverySettings.Bucket, DataType = "String" });
            request.MessageAttributes.Add(Constants.PubSubKey, new Amazon.SimpleNotificationService.Model.MessageAttributeValue { StringValue = key, DataType = "String" });
        }

        internal static async Task PubSubS3Query(this IAmazonS3 s3Client, SendMessageRequest request, IMessageDeliverySettings deliverySettings = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = await UploadToS3(s3Client, request, deliverySettings, cancellationToken);
            request.MessageBody = "#";
//            request.MessageAttributes = new Dictionary<string, Amazon.SQS.Model.MessageAttributeValue>();
            request.MessageAttributes.Add(Constants.PubSubBucket, new Amazon.SQS.Model.MessageAttributeValue { StringValue = deliverySettings.Bucket, DataType = "String" });
            request.MessageAttributes.Add(Constants.PubSubKey, new Amazon.SQS.Model.MessageAttributeValue { StringValue = key, DataType = "String" });
        }

        private static async Task<string> UploadToS3(IAmazonS3 s3Client, object obj, IMessageDeliverySettings deliverySettings = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            deliverySettings = deliverySettings ?? new MessageDeliverySettings();
            var key = Guid.NewGuid().ToString();
            var putRequest = new PutObjectRequest
            {
                BucketName = deliverySettings.Bucket,
                Key = key,
                ContentBody = JsonConvert.SerializeObject(obj)
            };
            var upload = await s3Client.PutObjectAsync(putRequest, cancellationToken);
            if (upload.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonS3Exception($"Error uploading to {deliverySettings.Bucket} with key {key}");
            }
            return key;
        }
    }
}