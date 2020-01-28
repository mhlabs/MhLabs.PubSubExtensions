namespace MhLabs.PubSubExtensions
{
    public class MessageAttributeDataType
    {
        public static MessageAttributeDataType String = new MessageAttributeDataType("String");
        public static MessageAttributeDataType StringArray = new MessageAttributeDataType("String.Array");
        public static MessageAttributeDataType Number = new MessageAttributeDataType("Number");

        public string Type { get; private set; }

        private MessageAttributeDataType(string type)
        {
            Type = type;
        }

    }
}