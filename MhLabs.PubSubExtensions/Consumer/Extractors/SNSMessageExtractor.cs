using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SNSMessageExtractor : IMessageExtractor
    {
        public Type ExtractorForType => typeof(SNSEvent);

        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) where TMessageType : class
        {
            var snsEvent = ev as SNSEvent;
            return await Task.FromResult(snsEvent.Records.Select(p => JsonConvert.DeserializeObject<TMessageType>(p.Sns.Message)));

        }
    }
}