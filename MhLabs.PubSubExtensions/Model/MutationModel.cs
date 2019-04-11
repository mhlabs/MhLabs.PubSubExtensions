using System;
using System.Collections.Generic;

namespace MhLabs.PubSubExtensions.Model
{
    public class MutationModel<T> where T : class, new()
    {
        public T OldImage { get; set; }
        public T NewImage { get; set; }

        public string EventType { set; get; }
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        public DateTime Published { get; set; } = DateTime.UtcNow;

        public List<string> Diff()
        {
            return OldImage.PropertyDiff(NewImage);
        }
    }
}