using Amazon.SimpleNotificationService.Model;

namespace MhLabs.PubSubExtensions
{
    public static class PublishRequestExtension
    {
        public static void DelaySeconds(this PublishRequest request, int seconds) {
            request.MessageAttributes.Add(Constants.DelaySeconds, new MessageAttributeValue {DataType = "String", StringValue = seconds.ToString()});
        }
        public static void DelayMinutes(this PublishRequest request, int minutes) {
            request.MessageAttributes.Add(Constants.DelaySeconds, new MessageAttributeValue {DataType = "String", StringValue = (minutes * 60).ToString()});
        }
        public static void DelayHours(this PublishRequest request, int hours) {
            request.MessageAttributes.Add(Constants.DelaySeconds, new MessageAttributeValue {DataType = "String", StringValue = (hours * 60 * 60).ToString()});
        }
        public static void DelayDays(this PublishRequest request, int days) {
            request.MessageAttributes.Add(Constants.DelaySeconds, new MessageAttributeValue {DataType = "String", StringValue = (days * 24 * 60 * 60).ToString()});
        }
    }
}