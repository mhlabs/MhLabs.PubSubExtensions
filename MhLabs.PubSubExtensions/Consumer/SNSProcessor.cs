using Amazon.Lambda.SNSEvents;
using Amazon.S3;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Microsoft.Extensions.Logging;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class SNSProcessor<TMessage> : MessageProcessorBase<SNSEvent, TMessage>
        where TMessage : class, new()
    {
        protected SNSProcessor(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null) : base(s3Client, loggerFactory)
        {
            RegisterExtractor(new SNSMessageExtractor<TMessage>());
        }
    }
}
