using Amazon.Lambda.SNSEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SNSMessageExtractor<TMessage> : IMessageExtractor<TMessage>
          where TMessage : class, new()
    {
        public Type ExtractorForType => typeof(SNSEvent);

        public async Task<IEnumerable<TMessage>> ExtractEventBody<TEvent>(TEvent ev)
        {
            var snsEvent = ev as SNSEvent;
            return await Task.FromResult(snsEvent.Records.Select(p => JsonConvert.DeserializeObject<TMessage>(p.Sns.Message)));
        }
    }
}