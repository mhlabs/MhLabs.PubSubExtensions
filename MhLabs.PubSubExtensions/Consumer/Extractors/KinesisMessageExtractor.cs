using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.KinesisEvents;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class KinesisMessageExtractor<TMessageType> : IMessageExtractor<TMessageType>
          where TMessageType : class, new()
    {
        public Type ExtractorForType => typeof(KinesisEvent);

        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType>(TEventType ev)
        {
            var kinesisEvent = ev as KinesisEvent;
            return await Task.FromResult(kinesisEvent.Records.Select(p => p.Kinesis.Data.DeserializeStream<TMessageType>()));
        }
    }
}