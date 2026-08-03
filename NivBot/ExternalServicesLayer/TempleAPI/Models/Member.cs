using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NivBot.ExternalServicesLayer.TempleAPI.Models
{
    public class Member
    {
        [JsonPropertyName("player")]
        public string Player { get; set; }

        [JsonPropertyName("items")]
        public Dictionary<string, int> Items { get; set; }
    }
}
