using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.KinesisEvents;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Newtonsoft.Json;
using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace MhLabs.PubSubExtensions.Consumer
{

    public abstract class MessageProcessorBase<TEventType, TMessageType>
    {

        private static IDictionary<Type, IMessageExtractor> _messageExtractorRegister = new Dictionary<Type, IMessageExtractor>();

        protected abstract Task HandleEvent(IEnumerable<TMessageType> items, ILambdaContext context);

        private readonly IAmazonS3 _s3Client;

        protected void RegisterExtractor(IMessageExtractor extractor)
        {
            _messageExtractorRegister.Add(extractor.ExtractorForType, extractor);
        }

        protected MessageProcessorBase(IAmazonS3 s3Client = null)
        {
            _s3Client = s3Client ?? new AmazonS3Client(RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")));
            RegisterExtractor(new SQSMessageExtractor());
            RegisterExtractor(new SNSMessageExtractor());
            RegisterExtractor(new KinesisMessageExtractor());
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task Process(TEventType ev, ILambdaContext context)
        {
            await ExtractMessage(ev);
            var rawData = await _messageExtractorRegister[ev.GetType()].ExtractEventBody<TEventType, TMessageType>(ev);
            await HandleEvent(rawData, context);
        }

        protected virtual async Task ExtractMessage(TEventType ev)
        {
            var sqs = ev as SQSEvent;
            var sns = ev as SNSEvent;

            // This is ugly, but it's because SQS and SNS have different MessageAttribute references to the same data structure
            if (sqs != null)
            {
                foreach (var record in sqs.Records)
                {

                    if (record.MessageAttributes.ContainsKey(S3Extension.PubSubBucket))
                    {
                        var bucket = record.MessageAttributes[S3Extension.PubSubBucket].StringValue;
                        var key = record.MessageAttributes[S3Extension.PubSubKey].StringValue;
                        var s3Response = await _s3Client.GetObjectAsync(bucket, key);
                        var json = await ReadStream(s3Response.ResponseStream);
                        var snsEvent = JsonConvert.DeserializeObject<SNSMessage>(json);
                        record.Body = snsEvent.Message;
                        foreach (var attribute in snsEvent.MessageAttributes)
                        {
                            record.MessageAttributes.Add(attribute.Key, new SQSEvent.MessageAttribute { DataType = "String", StringValue = attribute.Value.Value });
                        }

                    }
                }
            }

            if (sns != null)
            {
                foreach (var record in sns.Records)
                {
                    if (record.Sns.MessageAttributes.ContainsKey(S3Extension.PubSubBucket))
                    {
                        var bucket = record.Sns.MessageAttributes[S3Extension.PubSubBucket].Value;
                        var key = record.Sns.MessageAttributes[S3Extension.PubSubKey].Value;
                        var s3Response = await _s3Client.GetObjectAsync(bucket, key);
                        var json = await ReadStream(s3Response.ResponseStream);
                        var snsEvent = JsonConvert.DeserializeObject<SNSMessage>(json);
                        record.Sns.Message = snsEvent.Message;
                        foreach (var attribute in snsEvent.MessageAttributes)
                        {
                            record.Sns.MessageAttributes.Add(attribute.Key, attribute.Value);
                        }
                    }
                }
            }
        }

        private async Task<string> ReadStream(Stream responseStream)
        {
            using (var reader = new StreamReader(responseStream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}


