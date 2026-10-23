namespace NivBot.ExternalServicesLayer.TempleAPI
{
    // Bound from the "Temple" section of appsettings.json / environment variables.
    public sealed class TempleOptions
    {
        // Your TempleOSRS group. Used by /syncgroupitems when no group id is given, by the periodic
        // collection log sync before roles are updated, and for Temple competitions. 0 = not set.
        public int GroupId { get; set; }

        // The group key from TempleOSRS, needed to create competitions for the group.
        public string GroupKey { get; set; } = "";
    }
}
