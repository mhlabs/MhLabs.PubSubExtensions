namespace MhLabs.PubSubExtensions.Model
{
    public class MutationModel<T>
    {
        public T OldImage { get; set; }
        public T NewImage { get; set; }
    }
}