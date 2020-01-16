using Amazon.Lambda.KinesisEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class KinesisMessageExtractor<TMessage> : IMessageExtractor<TMessage>
          where TMessage : class, new()
    {
        public async Task<IEnumerable<TMessage>> ExtractEventBody<TEvent>(TEvent ev)
        {
            var kinesisEvent = ev as KinesisEvent;
            return await Task.FromResult(kinesisEvent.Records.Select(p => p.Kinesis.Data.DeserializeStream<TMessage>()));
        }
    }
}