using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public interface IMessageExtractor<TMessageType>
         where TMessageType : class, new()
    {
        Type ExtractorForType { get; }
        Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType>(TEventType ev);
    }
}