using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using MhLabs.PubSubExtensions.Model;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class SQSMessageExtractor<TMessageType> : IMessageExtractor<SQSMessageEnvelope<TMessageType>>
          where TMessageType : class, new()
    {
        public Type ExtractorForType => typeof(SQSEvent);

        public async Task<IEnumerable<SQSMessageEnvelope<TMessageType>>> ExtractEventBody<TEventType>(TEventType ev)
        {
            var sqsEvent = ev as SQSEvent;
            return await Task.FromResult(sqsEvent.Records.Select(p => new SQSMessageEnvelope<TMessageType>
            {
                Message = JsonConvert.DeserializeObject<TMessageType>(p.Body),
                MessageId = p.MessageId
            }));
        }
    }
}