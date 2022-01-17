using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Consumer;

public abstract class MessageProcessorBase<TEventType>
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger _logger;

    protected MessageProcessorBase(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null)
    {
        _s3Client = s3Client ?? new AmazonS3Client(RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION")));

       _logger = loggerFactory == null ? NullLogger.Instance : loggerFactory.CreateLogger(GetType());
    }

    protected virtual Task<HandleErrorResult> HandleError(TEventType ev, ILambdaContext context, Exception exception)
    {
        return Task.FromResult(HandleErrorResult.Throw);
    }

    protected void LogError(TEventType ev, Exception exception, ILambdaContext context)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(ev);
            var eventType = ev?.GetType();

            if (_logger == NullLogger.Instance)
            {
                context.Logger.Log($"Error when processing message type: {eventType}. Raw message: {payload}");
            }
            else
            {
                _logger.LogError(exception,
                    "Error when processing message type: {TEventType}. Raw message: {TEvent}",
                    eventType, payload);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during LogError: {ex}");
        }
    }

    protected virtual async Task PreparePubSubMessage(TEventType ev)
    {
        if (typeof(TEventType) != typeof(SQSEvent) && typeof(TEventType) != typeof(SNSEvent))
        {
            return;
        }

        // This is ugly, but it's because SQS and SNS have different MessageAttribute references to the same data structure
        if (ev is SQSEvent sqs)
        {
            await PreparePubSubMessage(sqs);
            return;
        }

        if (ev is SNSEvent sns)
        {
            await PreparePubSubMessage(sns);
            return;
        }
    }

    protected virtual async Task PreparePubSubMessage(SQSEvent sqs)
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
            var s3Response = await _s3Client.GetObjectAsync(bucket, key);
            var json = await ReadStream(s3Response.ResponseStream);
            var snsEvent = JsonConvert.DeserializeObject<SNSEvent.SNSMessage>(json);
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

    protected virtual async Task PreparePubSubMessage(SNSEvent sns)
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
            var s3Response = await _s3Client.GetObjectAsync(bucket, key);
            var json = await ReadStream(s3Response.ResponseStream);
            var snsEvent = JsonConvert.DeserializeObject<SNSEvent.SNSMessage>(json);
            record.Sns.Message = snsEvent.Message;

            LambdaLogger.Log("Adding SNS message attributes to record");
            foreach (var attribute in snsEvent.MessageAttributes)
            {
                if (!record.Sns.MessageAttributes.ContainsKey(attribute.Key))
                {
                    record.Sns.MessageAttributes.Add(attribute.Key, attribute.Value);
                }
            }
            if (record.Sns.MessageAttributes.Any())
            {
                LambdaLogger.Log($"mathem.env:sns.message_attributes:{string.Join(",", record.Sns.MessageAttributes.SelectMany(p => $"{p.Key}={p.Value?.Value?.Replace("=", "%3D")}"))}");
            }

        }
    }

    private async Task<string> ReadStream(Stream responseStream)
    {
        using(var reader = new StreamReader(responseStream))
        {
            return await reader.ReadToEndAsync();
        }
    }

    protected enum HandleErrorResult
    {
        Throw,
        ErrorHandledByConsumer
    }
}