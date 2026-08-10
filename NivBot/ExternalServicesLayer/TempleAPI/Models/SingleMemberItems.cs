using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NivBot.ExternalServicesLayer.TempleAPI.Models
{
    public class SingleMemberItems
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("count")]
        public int Amount { get; set; }
    }
}
