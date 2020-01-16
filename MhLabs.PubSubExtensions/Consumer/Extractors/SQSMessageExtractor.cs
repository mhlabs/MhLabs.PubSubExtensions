using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SQSMessageExtractor<TMessage> : IMessageExtractor<TMessage>
          where TMessage : class, new()
    {
        public async Task<IEnumerable<TMessage>> ExtractEventBody<TEvent>(TEvent ev)
        {
            var sqsEvent = ev as SQSEvent;
            return await Task.FromResult(sqsEvent.Records.Select(p => JsonConvert.DeserializeObject<TMessage>(p.Body)));
        }
    }
}