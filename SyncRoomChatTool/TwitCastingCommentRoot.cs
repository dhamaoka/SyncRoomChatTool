using Newtonsoft.Json;
using System.Collections.Generic;

namespace SyncRoomChatTool
{
    internal class TwitCastingCommentRoot
    {
        [JsonProperty("movie_id")]
        public string MovieId { get; set; }
        [JsonProperty("all_count")]
        public int AllCount { get; set; }
        [JsonProperty("comments")]
        public List<TwitCastingComment> Comments { get; set; }
    }
}
