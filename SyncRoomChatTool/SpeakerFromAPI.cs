﻿using System.Collections.Generic;

namespace SyncRoomChatTool
{
    internal class SpeakerFromAPI
    {
        public string Name { get; set; }
        public string Speaker_uuid { get; set; }
        public List<StyleFromAPI> Styles { get; set; }
        public string Version { get; set; }
    }
}
