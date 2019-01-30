using System;
using Amazon.SimpleNotificationService.Model;
using MhLabs.PubSubExtensions.Model;
using Newtonsoft.Json;

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

        internal static string GetExecutionName(this PublishRequest request)
        {
            return request.MessageAttributes.ContainsKey(Constants.StepFunctionsName)
                ? request.MessageAttributes[Constants.StepFunctionsName].StringValue
                : Guid.NewGuid().ToString();
        }

        internal static bool ShouldSuppressExecutionAlreadyExistsException(this PublishRequest request)
        {
            return request.MessageAttributes.ContainsKey(Constants.SuppressExecutionAlreadyExistsException) && 
                   request.MessageAttributes[Constants.SuppressExecutionAlreadyExistsException].StringValue == true.ToString();
        }

        public static void SuppressExecutionAlreadyExistsException(this PublishRequest request, bool suppress = true)
        {
            request.MessageAttributes.Add(Constants.SuppressExecutionAlreadyExistsException, new MessageAttributeValue
            {
                DataType = "String",
                StringValue = suppress.ToString()
            });
        }
        public static void SetVersion(this PublishRequest request, string version)
        {
            request.MessageAttributes.Add(Constants.Version, new MessageAttributeValue
            {
                DataType = "String",
                StringValue = version
            });
        }

        public static void AddMutation<T>(this PublishRequest request, T oldImage, T newImage) where T : class, new()
        {
            var model = new MutationModel<T> {
                OldImage = oldImage,
                NewImage = newImage
            };

            request.Message = JsonConvert.SerializeObject(model);
            request.MessageAttributes.Add(
                Constants.UpdatedProperties, 
                new MessageAttributeValue {
                    DataType = "String.Array", 
                    StringValue = JsonConvert.SerializeObject(model.Diff())
                }
            );
        }        
    }
}