using System.Collections.Generic;

namespace MhLabs.PubSubExtensions.Model
{
    public class SQSResponse
    {
        public List<BatchItemFailure> BatchItemFailures { get; set; }
    }

    public class BatchItemFailure
    {
        public string ItemIdentifier { get; set; }
    }
}
