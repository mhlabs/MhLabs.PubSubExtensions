using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SQSMessageExtractor : IMessageExtractor
    {
        public Type ExtractorForType => typeof(SQSEvent);

        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) {
            var sqsEvent = ev as SQSEvent;            
            return await Task.FromResult(sqsEvent.Records.Select(p => JsonConvert.DeserializeObject<TMessageType>(p.Body)));
        }

    }
}