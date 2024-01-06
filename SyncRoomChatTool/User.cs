using Newtonsoft.Json;

namespace SyncRoomChatTool
{
    internal class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("screen_id")]
        public string ScreenId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("profile")]
        public string Profile { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("last_movie_id")]
        public string LastMovieId { get; set; }
        [JsonProperty("is_live")]
        public bool IsAlive { get; set; }
        [JsonProperty("SupporterCount")]
        public int SupporterCount { get; set; }
        [JsonProperty("supporting_count")]
        public int SupportingCount { get; set; }
        [JsonProperty("created")]
        public int Created { get; set; }
    }
}
