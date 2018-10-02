using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions
{
    public static class StreamExtension
    {
        public static T DeserializeStream<T>(this Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }
    }
}
 