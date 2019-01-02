using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public interface IMessageExtractor
    {
        Type ExtractorForType { get; }
        Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) where TMessageType : class;
    }
}