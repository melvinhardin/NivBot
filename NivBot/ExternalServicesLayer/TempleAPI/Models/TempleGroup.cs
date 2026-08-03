using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NivBot.ExternalServicesLayer.TempleAPI.Models
{
    public class TempleGroup
    {
        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }
        [JsonPropertyName("group_name")]
        public string GroupName { get; set; }
        [JsonPropertyName("members")]
        public IList<Member> Members { get; set; }
    }
}
