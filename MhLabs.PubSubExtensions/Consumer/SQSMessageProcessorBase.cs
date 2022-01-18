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

    /// <summary>
    /// This class receives SQS events and invokes the HandleEvent method with deserialised objects
    /// of the type TMessageType. If the message contained a large message it will automatically
    /// download it from S3. If you configure the lambda to handle partial batch failure you can
    /// return the failed message ids on the SQS response. Make sure to also override HandleError
    /// and set the return value to ErrorHandledByConsumer.
    /// </summary>
    /// <typeparam name="TMessageType">The type of the message in your SQS message</typeparam>
    public abstract class SQSMessageProcessorBase<TMessageType> : MessageProcessorBase<SQSEvent>
        where TMessageType : class, new()
    {
        private IMessageExtractor<SQSMessageEnvelope<TMessageType>> _messageExtractor;

        protected SQSMessageProcessorBase(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null) : base(
            s3Client, loggerFactory)
        {
            _messageExtractor = new SQSMessageExtractor<TMessageType>();
        }

        protected abstract Task<SQSResponse> HandleEvent(IEnumerable<SQSMessageEnvelope<TMessageType>> items,
            ILambdaContext context);

        protected virtual Task<SQSResponse> HandleRawEvent(SQSEvent items, ILambdaContext context)
        {
            return Task.FromResult(new SQSResponse());
        }

        protected void RegisterExtractor(IMessageExtractor<SQSMessageEnvelope<TMessageType>> extractor)
        {
            _messageExtractor = extractor;
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<SQSResponse> Process(SQSEvent ev, ILambdaContext context)
        {
            try
            {
                await PreparePubSubMessage(ev);

                var rawData = await _messageExtractor.ExtractEventBody(ev);

                var response = await HandleEvent(rawData, context);
                var responseFromRaw = await HandleEvent(rawData, context);

                var failures = new List<BatchItemFailure>();
                failures.AddRange(response.BatchItemFailures);
                failures.AddRange(responseFromRaw.BatchItemFailures);

                return new SQSResponse
                {
                    BatchItemFailures = failures
                };
            }
            catch (Exception exception)
            {
                LogError(ev, exception, context);

                var result = await HandleError(ev, context, exception);
                if (result == HandleErrorResult.Throw)
                {
                    throw;
                }

                return new SQSResponse
                {
                    BatchItemFailures = ev.Records.Select(x => new BatchItemFailure {ItemIdentifier = x.MessageId})
                        .ToList()
                };
            }
        }
    }
}