using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class MessageProcessorBase<TEvent, TMessage> where TMessage : class, new()
    {
        private IMessageExtractor<TEvent, TMessage> _messageExtractor;

#pragma warning disable CS0618 // Type or member is obsolete
        private IMessageExtractor _deprecatedExtractor;
#pragma warning restore CS0618 // Type or member is obsolete

        protected abstract Task HandleEvent(IEnumerable<TMessage> items, ILambdaContext context);

        protected virtual async Task HandleRawEvent(TEvent items, ILambdaContext context)
        {
            await Task.CompletedTask;
        }

        internal IAmazonS3 S3Client { get; }
        private readonly ILogger _logger;

        protected void RegisterExtractor(IMessageExtractor<TEvent, TMessage> extractor)
        {
            _messageExtractor = extractor;
        }

        [Obsolete("Use IMessageExtractor<TMessageType> interface instead")]
        protected void RegisterExtractor(IMessageExtractor extractor)
        {
            _deprecatedExtractor = extractor;
        }

        protected MessageProcessorBase(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null)
        {
            S3Client = s3Client ?? new AmazonS3Client(RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION")));

            _logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task Process(TEvent ev, ILambdaContext context)
        {
            try
            {
                await PreparePubSubMessage(ev);

                IEnumerable<TMessage> rawData;
                if (_deprecatedExtractor != default)
                {
                    rawData = await _deprecatedExtractor.ExtractEventBody<TEvent, TMessage>(ev);
                }
                else
                {
                    rawData = await _messageExtractor.ExtractEventBody(ev);
                }

                await HandleEvent(rawData, context);
                await HandleRawEvent(ev, context);
            }
            catch (Exception exception)
            {
                LogError(ev, exception, context);

                var result = await HandleError(ev, context, exception);
                if (result == HandleErrorResult.Throw)
                {
                    throw;
                }
            }
        }

        protected virtual Task<HandleErrorResult> HandleError(TEvent ev, ILambdaContext context, Exception exception)
        {
            return Task.FromResult(HandleErrorResult.Throw);
        }

        protected enum HandleErrorResult
        {
            Throw,
            ErrorHandledByConsumer
        }

        private void LogError(TEvent ev, Exception exception, ILambdaContext context)
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

        protected virtual Task PreparePubSubMessage(TEvent ev)
        {
            return Task.CompletedTask;
        }

        internal async Task<string> ReadStream(Stream responseStream)
        {
            using (var reader = new StreamReader(responseStream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}