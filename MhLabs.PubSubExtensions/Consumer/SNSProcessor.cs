using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.S3;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class SNSProcessor<TMessage> : MessageProcessorBase<SNSEvent, TMessage>
        where TMessage : class, new()
    {
        protected SNSProcessor(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null)
            : base(s3Client, loggerFactory)
        {
            RegisterExtractor(new SNSMessageExtractor<TMessage>());
        }

        protected override async Task PreparePubSubMessage(SNSEvent sns)
        {
            LambdaLogger.Log($"Starting to process {sns.Records.Count} SNS records...");
            foreach (var record in sns.Records)
            {
                if (!record.Sns.MessageAttributes.ContainsKey(Constants.PubSubBucket))
                {
                    continue;
                }

                LambdaLogger.Log($"The records message attributes contains key {Constants.PubSubBucket}");
                var bucket = record.Sns.MessageAttributes[Constants.PubSubBucket].Value;
                var key = record.Sns.MessageAttributes[Constants.PubSubKey].Value;
                var s3Response = await S3Client.GetObjectAsync(bucket, key);
                var json = await ReadStream(s3Response.ResponseStream);
                var snsEvent = JsonConvert.DeserializeObject<SNSMessage>(json);
                record.Sns.Message = snsEvent.Message;

                LambdaLogger.Log("Adding SNS message attributes to record");
                foreach (var attribute in snsEvent.MessageAttributes)
                {
                    if (!record.Sns.MessageAttributes.ContainsKey(attribute.Key))
                    {
                        record.Sns.MessageAttributes.Add(attribute.Key, attribute.Value);
                    }
                }
                if (record.Sns.MessageAttributes.Count > 0)
                {
                    LambdaLogger.Log($"mathem.env:sns.message_attributes:{string.Join(",", record.Sns.MessageAttributes.SelectMany(p => $"{p.Key}={p.Value?.Value?.Replace("=", "%3D")}"))}");
                }
            }
        }
    }
}
