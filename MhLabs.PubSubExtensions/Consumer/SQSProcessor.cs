using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Microsoft.Extensions.Logging;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class SQSProcessor<TMessage> : MessageProcessorBase<SQSEvent, TMessage>
        where TMessage : class, new()
    {
        protected SQSProcessor(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null) : base(s3Client, loggerFactory)
        {
            RegisterExtractor(new SQSMessageExtractor<TMessage>());
        }
    }
}
