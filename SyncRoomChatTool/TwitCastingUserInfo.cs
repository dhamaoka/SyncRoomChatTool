using Newtonsoft.Json;

namespace SyncRoomChatTool
{
    internal class TwitCastingUserInfo
    {
        public string Supporter_count { get; set; }
        public string Supporting_count { get; set; }
        [JsonProperty("user")]
        public User UserInfo { get; set; }
    }
}
