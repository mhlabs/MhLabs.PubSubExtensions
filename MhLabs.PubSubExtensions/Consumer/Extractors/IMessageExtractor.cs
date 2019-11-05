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

    [Obsolete("Use IMessageExtractor<TMessageType> interface instead")]
    public interface IMessageExtractor
    {
        Type ExtractorForType { get; }
        Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) where TMessageType : class, new();
    }
}