using Newtonsoft.Json;

namespace Barriot.Http.Json
{
    public class TranslationRequest
    {
        [JsonProperty("q")]
        public string Text { get; set; } = "";

        [JsonProperty("source")]
        public string Source { get; set; } = "";

        [JsonProperty("target")]
        public string Target { get; set; } = "";

        [JsonProperty("api_key")]
        public string ApiKey { get; set; } = "";
    }
}
