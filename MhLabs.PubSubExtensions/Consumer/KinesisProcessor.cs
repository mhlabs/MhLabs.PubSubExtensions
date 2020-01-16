using Amazon.Lambda.KinesisEvents;
using Amazon.S3;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using Microsoft.Extensions.Logging;

namespace MhLabs.PubSubExtensions.Consumer
{
    public abstract class KinesisProcessor<TMessage> : MessageProcessorBase<KinesisEvent, TMessage>
        where TMessage : class, new()
    {
        protected KinesisProcessor(IAmazonS3 s3Client = null, ILoggerFactory loggerFactory = null) : base(s3Client, loggerFactory)
        {
            RegisterExtractor(new KinesisMessageExtractor<TMessage>());
        }
    }
}
