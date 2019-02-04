using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.KinesisEvents;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class KinesisMessageExtractor : IMessageExtractor
    {
        public Type ExtractorForType => typeof(KinesisEvent);

        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev)  where TMessageType : class, new() {
            var kinesisEvent = ev as KinesisEvent;            
            return await Task.FromResult(kinesisEvent.Records.Select(p => p.Kinesis.Data.DeserializeStream<TMessageType>()));
        }

    }
}