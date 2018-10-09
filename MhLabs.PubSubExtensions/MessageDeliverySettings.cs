using System;

namespace MhLabs.PubSubExtensions
{
    public class MessageDeliverySettings
    {
        public string Bucket { get; set; } = Environment.GetEnvironmentVariable("PubSub_Bucket");
        public string StateMachine { get; set; } = Environment.GetEnvironmentVariable("PubSub_StateMachine");
    }
}

