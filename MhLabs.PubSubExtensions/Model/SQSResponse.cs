﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Model
{
    public class SQSResponse
    {
        [JsonProperty(PropertyName = "batchItemFailures")]
        public List<BatchItemFailure> BatchItemFailures { get; set; }
    }

    public class BatchItemFailure
    {
        [JsonProperty(PropertyName = "itemIdentifier")]
        public string ItemIdentifier { get; set; }
    }
}