namespace MhLabs.PubSubExtensions
{
    public interface IMessageDeliverySettings
    {
        string Bucket { get; set; }
        string StateMachine { get; set; }
    }
}