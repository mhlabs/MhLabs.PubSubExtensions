namespace MhLabs.PubSubExtensions.Model
{
    public class SQSMessageEnvelope<T> where T : class, new()
    {
        public string MessageId { get; set; }
        public T Message { get; set; }
    }
}
