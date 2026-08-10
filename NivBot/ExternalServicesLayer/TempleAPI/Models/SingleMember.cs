using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NivBot.ExternalServicesLayer.TempleAPI.Models
{
    public class SingleMember
    {
        [JsonPropertyName("player")]
        public string Player { get; set; }
        [JsonPropertyName("items")]
        public IList<SingleMemberItems> Items { get; set; }

    }
}
