using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using MhLabs.PubSubExtensions.Model;
using Microsoft.Extensions.Logging;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class SQSMessageProcessorBase<TMessageType> : MessageProcessorBase<SQSEvent, TMessageType> where TMessageType : class, new()
    {
        private IMessageExtractor<SQSMessageEnvelope<TMessageType>> _messageExtractor;

        protected abstract Task<SQSResponse> HandleEvent(IEnumerable<SQSMessageEnvelope<TMessageType>> items, ILambdaContext context);

        protected override Task HandleEvent(IEnumerable<TMessageType> items, ILambdaContext context)
        {
            throw new NotImplementedException();
        }

        protected SQSMessageProcessorBase(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null) : base(s3Client, loggerFactory)
        {
            _messageExtractor = new SQSMessageExtractorForPartialFailure<TMessageType>();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public new async Task<SQSResponse> Process(SQSEvent ev, ILambdaContext context)
        {
            try
            {
                await PreparePubSubMessage(ev);

                var rawData = await _messageExtractor.ExtractEventBody(ev);

                return await HandleEvent(rawData, context);
            }
            catch (Exception exception)
            {
                LogError(ev, exception, context);

                return new SQSResponse
                {
                    BatchItemFailures = ev.Records.Select(x => new BatchItemFailure { ItemIdentifier = x.MessageId }).ToList()
                };
            }
        }
    }
}