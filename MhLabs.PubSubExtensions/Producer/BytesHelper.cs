using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS.Model;

namespace MhLabs.PubSubExtensions.Producer
{
    public static class BytesHelper
    {
        private const int DEFAULT_MESSAGE_SIZE_THRESHOLD = 256000;
        public static bool TooLarge(PublishRequest request, int maxSize = DEFAULT_MESSAGE_SIZE_THRESHOLD)
        {
            return TooLarge(request.Message, request.MessageAttributes.Select(p => new KeyValuePair<string, string>(p.Key, p.Value.StringValue)).ToDictionary(ks => ks.Key, x => x.Value), maxSize);
        }

        public static bool TooLarge(SendMessageRequest request, int maxSize = DEFAULT_MESSAGE_SIZE_THRESHOLD)
        {
            return TooLarge(request.MessageBody, request.MessageAttributes.Select(p => new KeyValuePair<string, string>(p.Key, p.Value.StringValue)).ToDictionary(ks => ks.Key, x => x.Value), maxSize);
        }

        private static bool TooLarge(string body, Dictionary<string, string> messageAttributes, int maxSize = DEFAULT_MESSAGE_SIZE_THRESHOLD)
        {
            long length = GetStringSizeInBytes(body);
            foreach (var attribute in messageAttributes)
            {
                length += GetStringSizeInBytes(attribute.Key + attribute.Value);
            }
            return length > maxSize;
        }

        private static long GetStringSizeInBytes(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream.Length; // TODO - assert this is correct way to measure
        }
    }
}