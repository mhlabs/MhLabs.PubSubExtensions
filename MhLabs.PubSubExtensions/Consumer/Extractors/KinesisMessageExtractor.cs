using Amazon.Lambda.KinesisEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class KinesisMessageExtractor<TMessage> : IMessageExtractor<KinesisEvent, TMessage>
          where TMessage : class, new()
    {
        public async Task<IEnumerable<TMessage>> ExtractEventBody(KinesisEvent ev)
        {
            return await Task.FromResult(ev.Records.Select(p => p.Kinesis.Data.DeserializeStream<TMessage>()));
        }
    }
}