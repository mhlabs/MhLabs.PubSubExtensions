using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public interface IMessageExtractor<TMessage>
         where TMessage : class, new()
    {
        Task<IEnumerable<TMessage>> ExtractEventBody<TEvent>(TEvent ev);
    }

    [Obsolete("Use IMessageExtractor<TMessageType> interface instead")]
    public interface IMessageExtractor
    {
        Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) where TMessageType : class, new();
    }
}