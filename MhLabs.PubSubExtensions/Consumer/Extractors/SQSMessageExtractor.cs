using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SQSMessageExtractor<TMessage> : IMessageExtractor<SQSEvent, TMessage>
          where TMessage : class, new()
    {
        public async Task<IEnumerable<TMessage>> ExtractEventBody(SQSEvent ev)
        {
            return await Task.FromResult(ev.Records.Select(p => JsonConvert.DeserializeObject<TMessage>(p.Body)));
        }
    }
}