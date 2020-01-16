using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SQS.Model;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class SQSProcessor<TMessage> : MessageProcessorBase<SQSEvent, TMessage>
        where TMessage : class, new()
    {
        protected SQSProcessor(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null) : base(s3Client, loggerFactory)
        {
            RegisterExtractor(new SQSMessageExtractor<TMessage>());
        }

        protected override async Task PreparePubSubMessage(SQSEvent sqs)
        {
            LambdaLogger.Log($"Starting to process {sqs.Records.Count} SQS records...");
            foreach (var record in sqs.Records)
            {
                if (!record.MessageAttributes.ContainsKey(Constants.PubSubBucket))
                {
                    continue;
                }

                LambdaLogger.Log($"The records message attributes contains key {Constants.PubSubBucket}");
                var bucket = record.MessageAttributes[Constants.PubSubBucket].StringValue;
                var key = record.MessageAttributes[Constants.PubSubKey].StringValue;
                var s3Response = await S3Client.GetObjectAsync(bucket, key);
                var json = await ReadStream(s3Response.ResponseStream);
                var snsEvent = JsonConvert.DeserializeObject<SNSMessage>(json);
                if (snsEvent?.Message != null && snsEvent.MessageAttributes != null)
                {
                    record.Body = snsEvent.Message;

                    LambdaLogger.Log("Adding SNS message attributes to record");
                    foreach (var attribute in snsEvent.MessageAttributes)
                    {
                        if (!record.MessageAttributes.ContainsKey(attribute.Key))
                        {
                            record.MessageAttributes.Add(attribute.Key, new SQSEvent.MessageAttribute { DataType = "String", StringValue = attribute.Value.Value });
                        }
                    }
                }
                else
                {
                    var sqsEvent = JsonConvert.DeserializeObject<SendMessageRequest>(json);
                    record.Body = sqsEvent.MessageBody;

                    LambdaLogger.Log("Adding SQS message attributes to record");
                    foreach (var attribute in sqsEvent.MessageAttributes)
                    {
                        if (!record.MessageAttributes.ContainsKey(attribute.Key))
                        {
                            record.MessageAttributes.Add(attribute.Key, new SQSEvent.MessageAttribute { DataType = "String", StringValue = attribute.Value.StringValue });
                        }
                    }
                }
            }
        }
    }
}
