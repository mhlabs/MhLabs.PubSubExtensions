using System;

namespace MhLabs.PubSubExtensions
{
    public class MessageDeliverySettings : IMessageDeliverySettings
    {
        public string Bucket { get; set; } = Environment.GetEnvironmentVariable("PubSub_Bucket");
        public string StateMachine { get; set; } = Environment.GetEnvironmentVariable("PubSub_StateMachine");
    }
}

