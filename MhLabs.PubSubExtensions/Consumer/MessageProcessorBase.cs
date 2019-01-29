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
using static Amazon.Lambda.SQSEvents.SQSEvent;

namespace MhLabs.PubSubExtensions.Consumer
{

    public abstract class MessageProcessorBase<TEventType, TMessageType> where TMessageType : class
    {

        private readonly IDictionary<Type, IMessageExtractor> _messageExtractorRegister = new Dictionary<Type, IMessageExtractor>();

        protected abstract Task HandleEvent(IEnumerable<TMessageType> items, ILambdaContext context);

        private readonly IAmazonS3 _s3Client;

        protected void RegisterExtractor(IMessageExtractor extractor)
        {
            if (!_messageExtractorRegister.ContainsKey(extractor.ExtractorForType))
            {
                _messageExtractorRegister.Add(extractor.ExtractorForType, extractor);
            }
            else
            {
                _messageExtractorRegister[extractor.ExtractorForType] = extractor;
            }
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
                LambdaLogger.Log($"Starting to process {sqs.Records.Count} SQS records...");
                foreach (var record in sqs.Records)
                {

                    if (record.MessageAttributes.ContainsKey(Constants.PubSubBucket))
                    {
                        LambdaLogger.Log($"The records message attributes contains key {Constants.PubSubBucket}");
                        var bucket = record.MessageAttributes[Constants.PubSubBucket].StringValue;
                        var key = record.MessageAttributes[Constants.PubSubKey].StringValue;
                        var s3Response = await _s3Client.GetObjectAsync(bucket, key);
                        var json = await ReadStream(s3Response.ResponseStream);
                        var snsEvent = JsonConvert.DeserializeObject<SNSMessage>(json);
                        if (snsEvent != null && snsEvent.Message != null && snsEvent.MessageAttributes != null)
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

            if (sns != null)
            {
                LambdaLogger.Log($"Starting to process {sns.Records.Count} SNS records...");
                foreach (var record in sns.Records)
                {
                    if (record.Sns.MessageAttributes.ContainsKey(Constants.PubSubBucket))
                    {
                        LambdaLogger.Log($"The records message attributes contains key {Constants.PubSubBucket}");
                        var bucket = record.Sns.MessageAttributes[Constants.PubSubBucket].Value;
                        var key = record.Sns.MessageAttributes[Constants.PubSubKey].Value;
                        var s3Response = await _s3Client.GetObjectAsync(bucket, key);
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


