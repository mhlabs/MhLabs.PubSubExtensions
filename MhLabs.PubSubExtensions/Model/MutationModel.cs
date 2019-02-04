using System.Collections.Generic;

namespace MhLabs.PubSubExtensions.Model
{
    public class MutationModel<T> where T : class, new()
    {
        public T OldImage { get; set; }
        public T NewImage { get; set; }

        public List<string> Diff()
        {
            return OldImage.PropertyDiff(NewImage);
        }
    }
}