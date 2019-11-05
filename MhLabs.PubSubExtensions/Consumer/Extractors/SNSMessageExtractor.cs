using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SNSMessageExtractor<TMessageType> : IMessageExtractor<TMessageType>
          where TMessageType : class, new()
    {
        public Type ExtractorForType => typeof(SNSEvent);

        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType>(TEventType ev)
        {
            var snsEvent = ev as SNSEvent;
            return await Task.FromResult(snsEvent.Records.Select(p => JsonConvert.DeserializeObject<TMessageType>(p.Sns.Message)));
        }
    }
}