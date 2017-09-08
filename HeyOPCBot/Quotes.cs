using Newtonsoft.Json;

namespace HeyOPCBot
{
    public class Quotes
    {
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; }
        [JsonProperty(PropertyName = "quote")]
        public string Quote { get; set; }
        [JsonProperty(PropertyName = "source_url")]
        public string URL { get; set; }
    }
}
