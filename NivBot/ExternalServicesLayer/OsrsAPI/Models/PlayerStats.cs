namespace NivBot.ExternalServicesLayer.OsrsAPI.Models
{
    public class PlayerStats
    {
        // Not required: the documented index_lite.json response only has "skills" and "activities" at the top level,
        // and System.Text.Json throws on a missing required member.
        public string? Name { get; set; }
        public required IList<Skills> Skills { get; set; }
        public required IList<Activities> Activities { get; set; }
    }
}
