using Newtonsoft.Json;

namespace Barriot.Http.Json
{
    public class Vote
    {
        [JsonProperty("bot")]
        public ulong BotId { get; set; }

        [JsonProperty("user")]
        public ulong UserId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("isWeekend")]
        public bool IsWeekend { get; set; }

        [JsonProperty("query")]
        public string? Query { get; set; }
    }
}
