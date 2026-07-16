using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NivBot.ExternalServicesLayer.OsrsAPI.Models
{
    public class PlayerStats
    {
        public required string Name { get; set; }
        public required IList<Skills> Skills { get; set; }
        public required IList<Activities> Activities { get; set; }
    }
}
