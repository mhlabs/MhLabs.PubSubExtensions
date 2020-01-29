using Amazon.Lambda.SNSEvents;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SNSMessageExtractor<TMessage> : IMessageExtractor<SNSEvent, TMessage>
          where TMessage : class, new()
    {
        public async Task<IEnumerable<TMessage>> ExtractEventBody(SNSEvent ev)
        {
            return await Task.FromResult(ev.Records.Select(p => JsonConvert.DeserializeObject<TMessage>(p.Sns.Message)));
        }
    }
}