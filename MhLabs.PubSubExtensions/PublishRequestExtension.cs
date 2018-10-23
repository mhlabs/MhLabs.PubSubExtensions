using System;
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

        public static void SetExecutionName(this PublishRequest request, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Execution Name cannot be null, empty or whitespace");

            if (name.Length > 80)
                throw new ArgumentException("Execution Name cannot be longer than 80 characters");

            request.MessageAttributes.Add(Constants.StepFunctionsName, new MessageAttributeValue {DataType = "String", StringValue = name});
        }
    }
}