using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.S3;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Microsoft.Extensions.Logging;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class SNSMessageMessageProcessorBase<TMessageType> : MessageProcessorBase<SNSEvent> where TMessageType : class, new()
    {
        private IMessageExtractor<TMessageType> _messageExtractor;

        protected SNSMessageMessageProcessorBase(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null) : base(s3Client, loggerFactory)
        {
            _messageExtractor = new SNSMessageExtractor<TMessageType>();
        }

        protected abstract Task HandleEvent(IEnumerable<TMessageType> items, ILambdaContext context);

        protected virtual async Task HandleRawEvent(SNSEvent items, ILambdaContext context)
        {
            await Task.CompletedTask;
        }

        protected void RegisterExtractor(IMessageExtractor<TMessageType> extractor)
        {
            _messageExtractor = extractor;
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task Process(SNSEvent ev, ILambdaContext context)
        {
            try
            {
                await PreparePubSubMessage(ev);
                var rawData = await _messageExtractor.ExtractEventBody(ev);

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
    }
}