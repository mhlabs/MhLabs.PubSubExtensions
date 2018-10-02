using System;

namespace MhLabs.PubSubExtensions
{
    public class S3MessageSettings
    {
        public string Bucket { get; set; } = Environment.GetEnvironmentVariable("PubSubBucket");
    }
}

