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
        public const string PubSubBucket = "PubSub_S3Bucket";
        public const string PubSubKey = "PubSub_S3Key";

        internal static async Task PubSubS3Query(this IAmazonS3 s3Client, PublishRequest request, S3MessageSettings s3Settings = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = await UploadToS3(s3Client, request, s3Settings, cancellationToken);            
            request.Message = "#";
            request.MessageAttributes = new Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue>();
            request.MessageAttributes.Add(PubSubBucket, new Amazon.SimpleNotificationService.Model.MessageAttributeValue { StringValue = s3Settings.Bucket, DataType = "String" });
            request.MessageAttributes.Add(PubSubKey, new Amazon.SimpleNotificationService.Model.MessageAttributeValue { StringValue = key, DataType = "String" });
        }

        internal static async Task PubSubS3Query(this IAmazonS3 s3Client, SendMessageRequest request, S3MessageSettings s3Settings = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = await UploadToS3(s3Client, request, s3Settings, cancellationToken);
            request.MessageBody = "#";
            request.MessageAttributes = new Dictionary<string, Amazon.SQS.Model.MessageAttributeValue>();
            request.MessageAttributes.Add(PubSubBucket, new Amazon.SQS.Model.MessageAttributeValue { StringValue = s3Settings.Bucket, DataType = "String" });
            request.MessageAttributes.Add(PubSubKey, new Amazon.SQS.Model.MessageAttributeValue { StringValue = key, DataType = "String" });
        }

        private static async Task<string> UploadToS3(IAmazonS3 s3Client, object obj, S3MessageSettings s3Settings = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            s3Settings = s3Settings ?? new S3MessageSettings();
            var key = Guid.NewGuid().ToString();
            var putRequest = new PutObjectRequest
            {
                BucketName = s3Settings.Bucket,
                Key = key,
                ContentBody = JsonConvert.SerializeObject(obj)
            };
            var upload = await s3Client.PutObjectAsync(putRequest, cancellationToken);
            if (upload.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonS3Exception($"Error uploading to {s3Settings.Bucket} with key {key}");
            }
            return key;
        }
    }
}