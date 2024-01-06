using Newtonsoft.Json;

namespace SyncRoomChatTool
{
    internal class TwitCastingComment
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("created")]
        public int Created { get; set; }
        [JsonProperty("from_user")]
        public User FromUser { get; set; }
    }
}
